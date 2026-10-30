using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.Application.Data;
using KooliProjekt.WebAPI.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.WebAPI.Services
{
    public sealed class CarRentalService : ICarRentalService
    {
        private const string Active = "active";
        private const string Completed = "completed";
        private readonly ApplicationDbContext _dbContext;

        public CarRentalService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<CarResponse>> ListAvailableCarsAsync(
            CancellationToken cancellationToken)
        {
            return await _dbContext.RentalCars
                .AsNoTracking()
                .Where(car => !_dbContext.Rentals.Any(
                    rental => rental.CarId == car.Id && rental.Status == Active))
                .OrderBy(car => car.Type)
                .ThenBy(car => car.Model)
                .Select(car => new CarResponse(
                    car.Id,
                    car.RegistrationNumber,
                    car.Model,
                    car.Type,
                    car.TimeRatePerMinute,
                    car.KilometerRate))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AdminCarResponse>> GetFleetAsync(
            CancellationToken cancellationToken)
        {
            return await _dbContext.RentalCars
                .AsNoTracking()
                .OrderBy(car => car.RegistrationNumber)
                .Select(car => new AdminCarResponse(
                    car.Id,
                    car.RegistrationNumber,
                    car.Model,
                    car.Type,
                    car.TimeRatePerMinute,
                    car.KilometerRate,
                    !_dbContext.Rentals.Any(
                        rental => rental.CarId == car.Id && rental.Status == Active)))
                .ToListAsync(cancellationToken);
        }

        public async Task<AdminCarResponse> GetCarAsync(
            int carId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.RentalCars
                .AsNoTracking()
                .Where(car => car.Id == carId)
                .Select(car => new AdminCarResponse(
                    car.Id,
                    car.RegistrationNumber,
                    car.Model,
                    car.Type,
                    car.TimeRatePerMinute,
                    car.KilometerRate,
                    !_dbContext.Rentals.Any(
                        rental => rental.CarId == car.Id && rental.Status == Active)))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<CarCreateResult> CreateCarAsync(
            CreateCarRequest request,
            CancellationToken cancellationToken)
        {
            var registrationNumber = request.RegistrationNumber.Trim().ToUpperInvariant();
            if (await _dbContext.RentalCars.AnyAsync(
                car => car.RegistrationNumber == registrationNumber,
                cancellationToken))
            {
                return CarCreateResult.RegistrationAlreadyExists;
            }

            var car = new RentalCar
            {
                RegistrationNumber = registrationNumber,
                Model = request.Model.Trim(),
                Type = request.Type.Trim(),
                TimeRatePerMinute = request.TimeRatePerMinute,
                KilometerRate = request.KilometerRate
            };
            _dbContext.RentalCars.Add(car);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                return CarCreateResult.RegistrationAlreadyExists;
            }

            return CarCreateResult.Created;
        }

        public async Task<IReadOnlyList<RentalResponse>> GetMyActiveRentalsAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var rentals = await _dbContext.Rentals
                .AsNoTracking()
                .Include(item => item.Car)
                .Include(item => item.Invoice)
                .ThenInclude(invoice => invoice.Line)
                .Where(item => item.UserId == userId && item.Status == Active)
                .OrderBy(item => item.Id)
                .ToListAsync(cancellationToken);

            return rentals.Select(ToRentalResponse).ToList();
        }

        public async Task<RentalResponse> GetMyRentalAsync(
            int rentalId,
            int userId,
            CancellationToken cancellationToken)
        {
            var rental = await _dbContext.Rentals
                .AsNoTracking()
                .Include(item => item.Car)
                .Include(item => item.Invoice)
                .ThenInclude(invoice => invoice.Line)
                .FirstOrDefaultAsync(
                    item => item.Id == rentalId && item.UserId == userId,
                    cancellationToken);

            return rental == null ? null : ToRentalResponse(rental);
        }

        public async Task<IReadOnlyList<RentalResponse>> GetMyRentalsAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var rentals = await _dbContext.Rentals
                .AsNoTracking()
                .Include(item => item.Car)
                .Include(item => item.Invoice)
                .ThenInclude(invoice => invoice.Line)
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.Id)
                .ToListAsync(cancellationToken);

            return rentals.Select(ToRentalResponse).ToList();
        }

        public async Task<RentalStartResult> StartAsync(
            int carId,
            int userId,
            CancellationToken cancellationToken)
        {
            var car = await _dbContext.RentalCars
                .FirstOrDefaultAsync(item => item.Id == carId, cancellationToken);

            if (car == null)
            {
                return new RentalStartResult(RentalStartStatus.CarNotFound, null);
            }

            if (await _dbContext.Rentals.AnyAsync(
                rental => rental.CarId == carId && rental.Status == Active,
                cancellationToken))
            {
                return new RentalStartResult(RentalStartStatus.CarUnavailable, null);
            }

            var rental = new Rental
            {
                CarId = carId,
                UserId = userId,
                StartedAt = DateTimeOffset.UtcNow,
                Status = Active
            };
            _dbContext.Rentals.Add(rental);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                return new RentalStartResult(RentalStartStatus.CarUnavailable, null);
            }

            return new RentalStartResult(RentalStartStatus.Started, rental.Id);
        }

        public async Task<FinishRentalResult> FinishAsync(
            int rentalId,
            int userId,
            decimal kilometersDriven,
            CancellationToken cancellationToken)
        {
            var rental = await _dbContext.Rentals
                .Include(item => item.Car)
                .FirstOrDefaultAsync(
                    item => item.Id == rentalId && item.UserId == userId && item.Status == Active,
                    cancellationToken);

            if (rental == null)
            {
                return FinishRentalResult.RentalNotFound;
            }

            var endedAt = DateTimeOffset.UtcNow;
            var minutes = (decimal)(endedAt - rental.StartedAt).Ticks
                / TimeSpan.TicksPerMinute;
            var total = decimal.Round(
                minutes * rental.Car.TimeRatePerMinute
                    + kilometersDriven * rental.Car.KilometerRate,
                2,
                MidpointRounding.AwayFromZero);

            rental.EndedAt = endedAt;
            rental.KilometersDriven = kilometersDriven;
            rental.Status = Completed;
            rental.Invoice = new Invoice
            {
                CreatedAt = endedAt,
                Total = total,
                IsPaid = false,
                Line = new InvoiceLine
                {
                    Description = $"Renditeenus {rental.Car.RegistrationNumber}",
                    Amount = total
                }
            };

            await _dbContext.SaveChangesAsync(cancellationToken);
            return FinishRentalResult.Finished;
        }

        public async Task<IReadOnlyList<AdminInvoiceResponse>> GetInvoicesAsync(
            CancellationToken cancellationToken)
        {
            var invoices = await _dbContext.Invoices
                .AsNoTracking()
                .Include(invoice => invoice.Line)
                .Include(invoice => invoice.Rental)
                .ThenInclude(rental => rental.Car)
                .Include(invoice => invoice.Rental)
                .ThenInclude(rental => rental.User)
                .OrderBy(invoice => invoice.IsPaid)
                .ThenByDescending(invoice => invoice.Id)
                .ToListAsync(cancellationToken);

            return invoices.Select(invoice => new AdminInvoiceResponse(
                invoice.Id,
                invoice.Rental.User.Name,
                invoice.Rental.User.Email,
                invoice.Rental.Car.RegistrationNumber,
                invoice.Line.Description,
                invoice.Total,
                invoice.IsPaid,
                invoice.CreatedAt,
                invoice.PaidAt)).ToList();
        }

        public async Task<MarkInvoicePaidResult> MarkInvoicePaidAsync(
            int invoiceId,
            CancellationToken cancellationToken)
        {
            var invoice = await _dbContext.Invoices
                .FirstOrDefaultAsync(item => item.Id == invoiceId, cancellationToken);

            if (invoice == null)
            {
                return MarkInvoicePaidResult.InvoiceNotFound;
            }

            if (invoice.IsPaid)
            {
                return MarkInvoicePaidResult.AlreadyPaid;
            }

            invoice.IsPaid = true;
            invoice.PaidAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return MarkInvoicePaidResult.Paid;
        }

        private static RentalResponse ToRentalResponse(Rental rental)
        {
            var car = new CarResponse(
                rental.Car.Id,
                rental.Car.RegistrationNumber,
                rental.Car.Model,
                rental.Car.Type,
                rental.Car.TimeRatePerMinute,
                rental.Car.KilometerRate);
            var invoice = rental.Invoice == null
                ? null
                : new InvoiceResponse(
                    rental.Invoice.Id,
                    rental.Invoice.CreatedAt,
                    rental.Invoice.Total,
                    rental.Invoice.IsPaid,
                    rental.Invoice.PaidAt,
                    new InvoiceLineResponse(
                        rental.Invoice.Line.Description,
                        rental.Invoice.Line.Amount));

            return new RentalResponse(
                rental.Id,
                rental.Status,
                rental.StartedAt,
                rental.EndedAt,
                rental.KilometersDriven,
                car,
                invoice);
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.GetBaseException() is SqliteException
            {
                SqliteErrorCode: 19,
                SqliteExtendedErrorCode: 1555 or 2067
            };
        }
    }
}
