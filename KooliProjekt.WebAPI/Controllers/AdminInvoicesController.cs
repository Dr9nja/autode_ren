using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.WebAPI.Contracts;
using KooliProjekt.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KooliProjekt.WebAPI.Controllers
{
    /// <summary>Allows administrators to review and mark rental invoices as paid.</summary>
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/invoices")]
    public class AdminInvoicesController : ControllerBase
    {
        private readonly ICarRentalService _rentalService;

        public AdminInvoicesController(ICarRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        /// <summary>Lists invoices with customer and payment details.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(AdminInvoiceResponse[]), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            return Ok(await _rentalService.GetInvoicesAsync(cancellationToken));
        }

        /// <summary>Marks an unpaid invoice as paid.</summary>
        [HttpPost("{id:int}/mark-paid")]
        [ProducesResponseType(typeof(AdminInvoiceResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> MarkPaid(int id, CancellationToken cancellationToken)
        {
            var result = await _rentalService.MarkInvoicePaidAsync(id, cancellationToken);
            if (result == MarkInvoicePaidResult.InvoiceNotFound)
            {
                return NotFound(new { message = "Invoice was not found." });
            }

            if (result == MarkInvoicePaidResult.AlreadyPaid)
            {
                return Conflict(new { message = "Invoice has already been marked as paid." });
            }

            var invoices = await _rentalService.GetInvoicesAsync(cancellationToken);
            var invoice = invoices.First(item => item.Id == id);
            return Ok(invoice);
        }

    }
}
