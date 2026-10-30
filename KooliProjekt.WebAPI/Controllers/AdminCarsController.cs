using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.WebAPI.Contracts;
using KooliProjekt.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KooliProjekt.WebAPI.Controllers
{
    /// <summary>Allows administrators to add cars to the rental fleet.</summary>
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/cars")]
    public class AdminCarsController : ControllerBase
    {
        private readonly ICarRentalService _rentalService;

        public AdminCarsController(ICarRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        /// <summary>Lists all cars and their current availability.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(AdminCarResponse[]), 200)]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            return Ok(await _rentalService.GetFleetAsync(cancellationToken));
        }

        /// <summary>Returns a car from the administrator's fleet.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AdminCarResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var car = await _rentalService.GetCarAsync(id, cancellationToken);
            return car == null ? NotFound() : Ok(car);
        }

        /// <summary>Adds a car to the fleet.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(AdminCarResponse), 201)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create(
            CreateCarRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _rentalService.CreateCarAsync(request, cancellationToken);
            if (result == CarCreateResult.RegistrationAlreadyExists)
            {
                return Conflict(new { message = "A car with this registration already exists." });
            }

            var fleet = await _rentalService.GetFleetAsync(cancellationToken);
            var car = fleet.First(item =>
                item.RegistrationNumber == request.RegistrationNumber.Trim().ToUpperInvariant());

            return CreatedAtAction(nameof(GetById), new { id = car.Id }, car);
        }
    }
}
