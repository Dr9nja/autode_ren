const bookingForm = document.querySelector("#booking-form");
const paymentStep = document.querySelector("#payment-step");
const bookingResult = document.querySelector("#booking-result");
const formMessage = document.querySelector("#form-message");
const paymentMessage = document.querySelector("#payment-message");
const resultMessage = document.querySelector("#result-message");
const paymentSummary = document.querySelector("#payment-summary");
const resultTitle = document.querySelector("#result-title");
const resultCopy = document.querySelector("#result-copy");
const returnButton = document.querySelector("#return-car");
const newBookingButton = document.querySelector("#new-booking");

let activeBookingId = null;

const tomorrow = new Date();
tomorrow.setDate(tomorrow.getDate() + 1);
tomorrow.setMinutes(tomorrow.getMinutes() - tomorrow.getTimezoneOffset());
document.querySelector("#booking-date").min = tomorrow.toISOString().slice(0, 10);

async function requestJson(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: { "Content-Type": "application/json", ...options.headers }
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
    throw new Error(body?.message || "The server rejected the request. Check the form and try again.");
  }
  return body;
}

function setBusy(buttons, busy) {
  for (const button of buttons) button.disabled = busy;
}

function showBookingResult(booking) {
  bookingResult.hidden = false;
  paymentStep.hidden = true;
  resultMessage.textContent = "";
  returnButton.hidden = booking.status !== "successful";
  newBookingButton.hidden = false;

  if (booking.status === "successful") {
    resultTitle.textContent = "Your car is being prepared.";
    resultCopy.textContent = `Booking #${booking.id} is confirmed for ${booking.bookingDate.slice(0, 10)}. ${booking.getToHome ? "We’ll deliver it to your address." : "Pick it up from our shop."} A booking notification is recorded for ${booking.customer.email}.`;
  } else if (booking.status === "cancelled") {
    resultTitle.textContent = "Booking cancelled.";
    resultCopy.textContent = `Booking #${booking.id} was cancelled. No payment was taken.`;
  } else {
    resultTitle.textContent = "Car returned.";
    resultCopy.textContent = `Booking #${booking.id} has been marked as returned. Thanks for choosing Auto Deren.`;
  }
}

bookingForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  formMessage.textContent = "";
  const submitButton = bookingForm.querySelector('button[type="submit"]');
  setBusy([submitButton], true);

  const formData = new FormData(bookingForm);
  const booking = Object.fromEntries(formData.entries());
  booking.bookingDate = `${booking.bookingDate}T00:00:00Z`;
  booking.getToHome = formData.has("getToHome");

  try {
    const created = await requestJson("/api/bookings", {
      method: "POST",
      body: JSON.stringify(booking)
    });
    activeBookingId = created.id;
    paymentSummary.textContent = `Your ${created.carModel} is reserved for ${created.bookingDate.slice(0, 10)}. Choose an option to continue.`;
    bookingForm.hidden = true;
    paymentStep.hidden = false;
    bookingResult.hidden = true;
    paymentMessage.textContent = "";
    paymentStep.scrollIntoView({ behavior: "smooth", block: "center" });
  } catch (error) {
    formMessage.textContent = error.message;
  } finally {
    setBusy([submitButton], false);
  }
});

async function processPayment(approved) {
  const buttons = [
    document.querySelector("#approve-payment"),
    document.querySelector("#cancel-payment")
  ];
  paymentMessage.textContent = "";
  setBusy(buttons, true);

  try {
    const booking = await requestJson(`/api/bookings/${activeBookingId}/payment`, {
      method: "POST",
      body: JSON.stringify({ approved })
    });
    showBookingResult(booking);
    bookingResult.scrollIntoView({ behavior: "smooth", block: "center" });
  } catch (error) {
    paymentMessage.textContent = error.message;
  } finally {
    setBusy(buttons, false);
  }
}

document.querySelector("#approve-payment").addEventListener("click", () => processPayment(true));
document.querySelector("#cancel-payment").addEventListener("click", () => processPayment(false));

newBookingButton.addEventListener("click", () => {
  activeBookingId = null;
  bookingForm.reset();
  bookingForm.hidden = false;
  bookingResult.hidden = true;
  formMessage.textContent = "";
  document.querySelector("#booking-date").min = tomorrow.toISOString().slice(0, 10);
});

returnButton.addEventListener("click", async () => {
  returnButton.disabled = true;
  resultMessage.textContent = "";
  try {
    const booking = await requestJson(`/api/bookings/${activeBookingId}/return`, { method: "POST" });
    showBookingResult(booking);
  } catch (error) {
    resultMessage.textContent = error.message;
  } finally {
    returnButton.disabled = false;
  }
});

requestJson("/api/shop")
  .then((shop) => {
    document.querySelector("#shop-name").textContent = shop.name;
    document.querySelector("#shop-address").textContent = `${shop.street}, ${shop.country}`;
    document.querySelector("#shop-contact").textContent = shop.email;
    document.querySelector("#shop-contact").href = `mailto:${shop.email}`;
    document.querySelector("#shop-email").href = `mailto:${shop.email}`;
  })
  .catch(() => {
    document.querySelector("#shop-address").textContent = "Shop details could not be loaded.";
    document.querySelector("#shop-contact").textContent = "Contact details unavailable.";
    document.querySelector("#shop-contact").removeAttribute("href");
    document.querySelector("#shop-email").textContent = "Contact unavailable";
    document.querySelector("#shop-email").removeAttribute("href");
  });
