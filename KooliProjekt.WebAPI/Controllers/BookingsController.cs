using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.WebAPI.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController : ControllerBase
    {
        private const string PendingPayment = "payment-pending";
        private const string Successful = "successful";
        private const string Cancelled = "cancelled";
        private const string Returned = "returned";
        private readonly ApplicationDbContext _dbContext;

        public BookingsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingRequest request)
        {
            if (request.BookingDate.Date <= DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "Choose a booking date in the future." });
            }

            var customer = new Customer
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                Address = new Address
                {
                    Street = request.Street.Trim(),
                    City = request.City.Trim(),
                    County = string.IsNullOrWhiteSpace(request.County) ? null : request.County.Trim(),
                    PostalCode = request.PostalCode.Trim(),
                    Country = request.Country.Trim()
                }
            };
            var order = new CarOrder
            {
                Customer = customer,
                CarModel = request.CarModel.Trim(),
                Status = PendingPayment,
                OrderDate = DateTime.SpecifyKind(request.BookingDate.Date, DateTimeKind.Utc),
                GetToHome = request.GetToHome
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            return StatusCode(201, ToResponse(order));
        }

        [HttpPost("{id:int}/payment")]
        public async Task<IActionResult> ProcessPayment(int id, ProcessPaymentRequest request)
        {
            var order = await _dbContext.Orders
                .Include(item => item.Customer)
                .ThenInclude(customer => customer.Address)
                .Include(item => item.Notifications)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != PendingPayment)
            {
                return Conflict(new { message = "This booking has already been processed." });
            }

            var paymentStatus = request.Approved.Value ? Successful : Cancelled;
            order.Status = paymentStatus;
            order.Payment = new Payment
            {
                Status = paymentStatus,
                OrderDate = order.OrderDate,
                GetToHome = order.GetToHome
            };

            if (request.Approved.Value)
            {
                order.Notifications.Add(new Notification
                {
                    Email = order.Customer.Email,
                    Description = "CAR PREPARED",
                    ReceiveDate = DateTime.UtcNow,
                    GetToHome = order.GetToHome
                });
            }

            await _dbContext.SaveChangesAsync();
            return Ok(ToResponse(order));
        }

        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return(int id)
        {
            var order = await _dbContext.Orders
                .Include(item => item.Customer)
                .ThenInclude(customer => customer.Address)
                .Include(item => item.Notifications)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != Successful)
            {
                return Conflict(new { message = "Only successful bookings can be returned." });
            }

            order.Status = Returned;
            await _dbContext.SaveChangesAsync();
            return Ok(ToResponse(order));
        }

        private static BookingResponse ToResponse(CarOrder order)
        {
            return new BookingResponse
            {
                Id = order.Id,
                CarModel = order.CarModel,
                Status = order.Status,
                BookingDate = order.OrderDate,
                GetToHome = order.GetToHome,
                Customer = new CustomerResponse
                {
                    Name = order.Customer.Name,
                    Email = order.Customer.Email,
                    Address = string.Join(", ", new[]
                    {
                        order.Customer.Address.Street,
                        order.Customer.Address.City,
                        order.Customer.Address.PostalCode,
                        order.Customer.Address.Country
                    }.Where(part => !string.IsNullOrWhiteSpace(part)))
                },
                Notifications = order.Notifications
                    .Select(notification => notification.Description)
                    .ToArray()
            };
        }
    }

    public class CreateBookingRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        [Phone]
        [StringLength(30)]
        public string Phone { get; set; }

        [Required]
        [StringLength(100)]
        public string CarModel { get; set; }

        [Required]
        [StringLength(200)]
        public string Street { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string County { get; set; }

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        public DateTime BookingDate { get; set; }
        public bool GetToHome { get; set; }
    }

    public class ProcessPaymentRequest
    {
        [Required]
        public bool? Approved { get; set; }
    }

    public class BookingResponse
    {
        public int Id { get; set; }
        public string CarModel { get; set; }
        public string Status { get; set; }
        public DateTime BookingDate { get; set; }
        public bool GetToHome { get; set; }
        public CustomerResponse Customer { get; set; }
        public string[] Notifications { get; set; }
    }

    public class CustomerResponse
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
