using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.WebAPI.Contracts;
using KooliProjekt.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KooliProjekt.WebAPI.Controllers
{
    /// <summary>Lists available cars and lets signed-in customers manage rentals.</summary>
    [ApiController]
    [Route("api")]
    public class RentalsController : ControllerBase
    {
        private readonly ICarRentalService _rentalService;

        public RentalsController(ICarRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        /// <summary>Lists cars that are not currently being rented.</summary>
        [AllowAnonymous]
        [HttpGet("cars")]
        [ProducesResponseType(typeof(CarResponse[]), 200)]
        public async Task<IActionResult> ListAvailableCars(CancellationToken cancellationToken)
        {
            return Ok(await _rentalService.ListAvailableCarsAsync(cancellationToken));
        }

        /// <summary>Starts a rental for an available car.</summary>
        [Authorize]
        [HttpPost("rentals")]
        [ProducesResponseType(typeof(RentalResponse), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Start(
            StartRentalRequest request,
            CancellationToken cancellationToken)
        {
            var userId = CurrentUserId();
            var result = await _rentalService.StartAsync(request.CarId, userId, cancellationToken);

            if (result.Status == RentalStartStatus.CarNotFound)
            {
                return NotFound(new { message = "Car was not found." });
            }

            if (result.Status == RentalStartStatus.CarUnavailable)
            {
                return Conflict(new { message = "This car is already being rented." });
            }

            var rental = await _rentalService.GetMyRentalAsync(
                result.RentalId.Value,
                userId,
                cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { id = rental.Id },
                rental);
        }

        /// <summary>Returns all rentals currently active for the signed-in customer.</summary>
        [Authorize]
        [HttpGet("rentals/mine")]
        [ProducesResponseType(typeof(RentalResponse[]), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        {
            return Ok(await _rentalService.GetMyActiveRentalsAsync(
                CurrentUserId(),
                cancellationToken));
        }

        /// <summary>Lists all rentals belonging to the signed-in customer.</summary>
        [Authorize]
        [HttpGet("rentals/history")]
        [ProducesResponseType(typeof(RentalResponse[]), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
        {
            return Ok(await _rentalService.GetMyRentalsAsync(
                CurrentUserId(),
                cancellationToken));
        }

        /// <summary>Returns one rental belonging to the signed-in customer.</summary>
        [Authorize]
        [HttpGet("rentals/{id:int}")]
        [ProducesResponseType(typeof(RentalResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var rental = await _rentalService.GetMyRentalAsync(
                id,
                CurrentUserId(),
                cancellationToken);

            return rental == null ? NotFound() : Ok(rental);
        }

        /// <summary>Ends a rental and creates its invoice from time and distance charges.</summary>
        [Authorize]
        [HttpPost("rentals/{id:int}/finish")]
        [ProducesResponseType(typeof(RentalResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Finish(
            int id,
            FinishRentalRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _rentalService.FinishAsync(
                id,
                CurrentUserId(),
                request.KilometersDriven,
                cancellationToken);

            if (result == FinishRentalResult.RentalNotFound)
            {
                return NotFound(new { message = "An active rental with this ID was not found." });
            }

            return Ok(await _rentalService.GetMyRentalAsync(
                id,
                CurrentUserId(),
                cancellationToken));
        }

        private int CurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
