const authCard = document.querySelector("#auth-card");
const authForm = document.querySelector("#auth-form");
const authMessage = document.querySelector("#auth-message");
const carList = document.querySelector("#car-list");
const fleetMessage = document.querySelector("#fleet-message");
const activeRentalSection = document.querySelector("#active-rental");
const activeRentalList = document.querySelector("#active-rental-list");
const historySection = document.querySelector("#history-section");
const historyList = document.querySelector("#history-list");
const adminPanel = document.querySelector("#admin-panel");

let account = null;
let isRegistering = false;

async function requestJson(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: options.body
      ? { "Content-Type": "application/json", ...options.headers }
      : options.headers
  });
  const text = await response.text();
  let body = null;
  if (text) {
    try {
      body = JSON.parse(text);
    } catch {
      if (!response.ok) throw new Error(text);
      throw new Error("The server returned an unreadable response.");
    }
  }
  if (!response.ok) {
    const error = new Error(
      body?.message || body?.detail || body?.title
      || (response.status === 401 ? "Unauthorized" : "The request could not be completed.")
    );
    error.status = response.status;
    throw error;
  }
  return body;
}

function element(tag, className, text) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text !== undefined) node.textContent = text;
  return node;
}

function formatCurrency(value) {
  return new Intl.NumberFormat("en-EE", { style: "currency", currency: "EUR" }).format(value);
}

function setBusy(buttons, busy) {
  for (const button of buttons) button.disabled = busy;
}

function setAccount(nextAccount) {
  account = nextAccount;
  const signedIn = account !== null;
  authCard.hidden = signedIn;
  document.querySelector("#account-banner").hidden = !signedIn;
  document.querySelector("#logout-button").hidden = !signedIn;
  document.querySelector("#signed-in-as").textContent = signedIn ? account.email : "";
  document.querySelector("#welcome-name").textContent = signedIn ? `Welcome, ${account.name}` : "";
  adminPanel.hidden = !account?.isAdmin;
  if (signedIn) {
    refreshCustomerData().catch((error) => {
      document.querySelector("#fleet-message").textContent = error.message;
    });
  } else {
    activeRentalSection.hidden = true;
    historySection.hidden = true;
    activeRentalList.replaceChildren();
    historyList.replaceChildren();
  }
  renderCars().catch((error) => {
    fleetMessage.textContent = error.message;
  });
  if (account?.isAdmin) refreshAdminData();
}

function formatDate(value) {
  return new Intl.DateTimeFormat("en-EE", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

async function renderCars() {
  fleetMessage.textContent = "Loading cars…";
  carList.replaceChildren();
  const cars = await requestJson("/api/cars");
  fleetMessage.textContent = cars.length ? `${cars.length} available` : "No cars are available right now.";

  for (const car of cars) {
    const card = element("article", "car-card");
    const carTop = element("div", "car-card-top");
    carTop.append(
      element("span", "car-type", car.type),
      element("span", "car-registration", car.registrationNumber)
    );
    card.append(carTop);
    card.append(element("h3", "car-model", car.model));
    const rates = element("div", "car-rates");
    rates.append(
      element("span", "", `${formatCurrency(car.timeRatePerMinute)} / minute`),
      element("span", "", `${formatCurrency(car.kilometerRate)} / km`)
    );
    card.append(rates);
    const reserve = element(
      "button",
      account ? "button button-primary car-rent" : "button button-secondary car-rent",
      account ? "Rent this car" : "Sign in to rent"
    );
    reserve.type = "button";
    reserve.addEventListener("click", async () => {
      if (!account) {
        authCard.scrollIntoView({ behavior: "smooth", block: "center" });
        document.querySelector("#account-email").focus();
        return;
      }

      setBusy([reserve], true);
      fleetMessage.textContent = "";
      try {
        await requestJson("/api/rentals", {
          method: "POST",
          body: JSON.stringify({ carId: car.id })
        });
        await renderCars();
        await refreshCustomerData();
      } catch (error) {
        fleetMessage.textContent = error.message;
        await renderCars();
      } finally {
        setBusy([reserve], false);
      }
    });
    card.append(reserve);
    carList.append(card);
  }
}

function renderActiveRentals(rentals) {
  activeRentalList.replaceChildren();
  activeRentalSection.hidden = rentals.length === 0;

  for (const rental of rentals) {
    const card = element("article", "active-rental-item");
    const car = element("div", "active-rental-car");
    car.append(
      element("div", "car-type", rental.car.type),
      element("h3", "", `${rental.car.model} · ${rental.car.registrationNumber}`),
      element("p", "", `Started ${formatDate(rental.startedAt)}`),
      element("p", "rate-note", `${formatCurrency(rental.car.timeRatePerMinute)} / minute + ${formatCurrency(rental.car.kilometerRate)} / km`)
    );
    const form = element("form", "finish-form");
    const field = element("div", "form-field");
    const label = element("label", "", "Kilometers driven");
    const inputId = `kilometers-${rental.id}`;
    label.htmlFor = inputId;
    const input = element("input");
    input.id = inputId;
    input.type = "number";
    input.name = "kilometersDriven";
    input.min = "0";
    input.max = "1000000";
    input.step = "0.1";
    input.required = true;
    field.append(label, input);
    const finish = element("button", "button button-primary", "Finish rental & create invoice");
    finish.type = "submit";
    form.append(field, finish);
    const message = element("p", "message");
    message.setAttribute("role", "status");
    form.addEventListener("submit", async (event) => {
      event.preventDefault();
      setBusy([finish], true);
      message.textContent = "";
      try {
        await requestJson(`/api/rentals/${rental.id}/finish`, {
          method: "POST",
          body: JSON.stringify({ kilometersDriven: Number(input.value) })
        });
        await refreshCustomerData();
        await renderCars();
      } catch (error) {
        message.textContent = error.message;
      } finally {
        setBusy([finish], false);
      }
    });
    card.append(car, form, message);
    activeRentalList.append(card);
  }
}

function renderHistory(rentals) {
  historyList.replaceChildren();
  historySection.hidden = !account;
  if (!rentals.length) {
    historyList.append(element("p", "empty-state", "Your completed rentals and invoices will appear here."));
    return;
  }

  for (const rental of rentals) {
    const item = element("article", "history-item");
    const detail = element("div", "history-detail");
    detail.append(
      element("strong", "", `${rental.car.model} · ${rental.car.registrationNumber}`),
      element("span", "", `${formatDate(rental.startedAt)}${rental.endedAt ? ` — ${formatDate(rental.endedAt)}` : " — In progress"}`)
    );
    item.append(detail);
    if (rental.invoice) {
      const invoice = element("div", "invoice-detail");
      invoice.append(
        element("strong", "", formatCurrency(rental.invoice.total)),
        element("span", rental.invoice.isPaid ? "paid-label" : "unpaid-label", rental.invoice.isPaid ? "Paid" : "Unpaid"),
        element("small", "", rental.invoice.line.description)
      );
      item.append(invoice);
    } else {
      item.append(element("span", "rental-status", "ACTIVE"));
    }
    historyList.append(item);
  }
}

async function refreshCustomerData() {
  const [activeRentals, rentals] = await Promise.all([
    requestJson("/api/rentals/mine"),
    requestJson("/api/rentals/history")
  ]);
  renderActiveRentals(activeRentals);
  renderHistory(rentals);
}

async function refreshAdminData() {
  if (!account?.isAdmin) return;
  const [fleet, invoices] = await Promise.all([
    requestJson("/api/admin/cars"),
    requestJson("/api/admin/invoices")
  ]);
  const fleetList = document.querySelector("#admin-fleet");
  fleetList.replaceChildren();
  for (const car of fleet) {
    const row = element("div", "admin-row");
    row.append(
      element("strong", "", `${car.registrationNumber} · ${car.model}`),
      element("span", "", `${car.type} · ${car.isAvailable ? "Available" : "Rented"}`),
      element("small", "", `${formatCurrency(car.timeRatePerMinute)} / min · ${formatCurrency(car.kilometerRate)} / km`)
    );
    fleetList.append(row);
  }

  const invoiceList = document.querySelector("#admin-invoices");
  invoiceList.replaceChildren();
  if (!invoices.length) {
    invoiceList.append(element("p", "empty-state", "No invoices yet."));
    return;
  }
  for (const invoice of invoices) {
    const row = element("div", "admin-row");
    const heading = element("strong", "", `${invoice.description} · ${formatCurrency(invoice.total)}`);
    const customer = element("span", "", `${invoice.customerName} · ${invoice.customerEmail}`);
    const status = element("span", invoice.isPaid ? "paid-label" : "unpaid-label", invoice.isPaid ? `Paid ${formatDate(invoice.paidAt)}` : "Unpaid");
    row.append(heading, customer, status);
    if (!invoice.isPaid) {
      const payButton = element("button", "button button-secondary", "Mark paid");
      payButton.type = "button";
      payButton.addEventListener("click", async () => {
        setBusy([payButton], true);
        document.querySelector("#admin-message").textContent = "";
        try {
          await requestJson(`/api/admin/invoices/${invoice.id}/mark-paid`, { method: "POST" });
          await refreshAdminData();
          await refreshCustomerData();
        } catch (error) {
          document.querySelector("#admin-message").textContent = error.message;
        } finally {
          setBusy([payButton], false);
        }
      });
      row.append(payButton);
    }
    invoiceList.append(row);
  }
}

document.querySelector("#auth-toggle").addEventListener("click", () => {
  isRegistering = !isRegistering;
  document.querySelector("#auth-title").textContent = isRegistering ? "Create your account" : "Sign in to rent a car";
  document.querySelector("#auth-toggle").textContent = isRegistering ? "I already have an account" : "Create account";
  document.querySelector("#auth-submit").textContent = isRegistering ? "Create account" : "Sign in";
  document.querySelector("#name-field").hidden = !isRegistering;
  document.querySelector("#account-name").required = isRegistering;
  document.querySelector("#account-password").minLength = isRegistering ? 12 : 1;
  document.querySelector("#account-password").autocomplete = isRegistering ? "new-password" : "current-password";
  authMessage.textContent = "";
});

authForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  authMessage.textContent = "";
  const submit = document.querySelector("#auth-submit");
  setBusy([submit], true);
  const data = new FormData(authForm);
  const body = {
    email: data.get("email"),
    password: data.get("password")
  };
  if (isRegistering) body.name = data.get("name");
  try {
    const nextAccount = await requestJson(`/api/account/${isRegistering ? "register" : "login"}`, {
      method: "POST",
      body: JSON.stringify(body)
    });
    authForm.reset();
    setAccount(nextAccount);
  } catch (error) {
    authMessage.textContent = error.message;
  } finally {
    setBusy([submit], false);
  }
});

document.querySelector("#logout-button").addEventListener("click", async () => {
  try {
    await requestJson("/api/account/logout", { method: "POST" });
    setAccount(null);
  } catch (error) {
    authMessage.textContent = error.message;
  }
});

document.querySelector("#add-car-form").addEventListener("submit", async (event) => {
  event.preventDefault();
  const form = event.currentTarget;
  const button = form.querySelector('button[type="submit"]');
  const message = document.querySelector("#admin-car-message");
  const data = new FormData(form);
  setBusy([button], true);
  message.textContent = "";
  try {
    await requestJson("/api/admin/cars", {
      method: "POST",
      body: JSON.stringify({
        registrationNumber: data.get("registrationNumber"),
        model: data.get("model"),
        type: data.get("type"),
        timeRatePerMinute: Number(data.get("timeRatePerMinute")),
        kilometerRate: Number(data.get("kilometerRate"))
      })
    });
    form.reset();
    await refreshAdminData();
    await renderCars();
  } catch (error) {
    message.textContent = error.message;
  } finally {
    setBusy([button], false);
  }
});

document.querySelector("#refresh-admin").addEventListener("click", () => {
  refreshAdminData().catch((error) => {
    document.querySelector("#admin-message").textContent = error.message;
  });
});

renderCars().catch((error) => {
  fleetMessage.textContent = error.message;
});

requestJson("/api/account/me")
  .then(setAccount)
  .catch((error) => {
    if (error.status !== 401) {
      authMessage.textContent = "Could not check the current sign-in session.";
    }
    setAccount(null);
  });
