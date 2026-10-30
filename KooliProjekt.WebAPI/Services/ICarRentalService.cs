using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.WebAPI.Contracts;

namespace KooliProjekt.WebAPI.Services
{
    public interface ICarRentalService
    {
        Task<IReadOnlyList<CarResponse>> ListAvailableCarsAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<AdminCarResponse>> GetFleetAsync(CancellationToken cancellationToken);
        Task<AdminCarResponse> GetCarAsync(int carId, CancellationToken cancellationToken);
        Task<CarCreateResult> CreateCarAsync(
            CreateCarRequest request,
            CancellationToken cancellationToken);
        Task<RentalResponse> GetMyRentalAsync(
            int rentalId,
            int userId,
            CancellationToken cancellationToken);
        Task<IReadOnlyList<RentalResponse>> GetMyActiveRentalsAsync(
            int userId,
            CancellationToken cancellationToken);
        Task<IReadOnlyList<RentalResponse>> GetMyRentalsAsync(int userId, CancellationToken cancellationToken);
        Task<RentalStartResult> StartAsync(int carId, int userId, CancellationToken cancellationToken);
        Task<FinishRentalResult> FinishAsync(
            int rentalId,
            int userId,
            decimal kilometersDriven,
            CancellationToken cancellationToken);
        Task<IReadOnlyList<AdminInvoiceResponse>> GetInvoicesAsync(CancellationToken cancellationToken);
        Task<MarkInvoicePaidResult> MarkInvoicePaidAsync(int invoiceId, CancellationToken cancellationToken);
    }

    public sealed record RentalStartResult(RentalStartStatus Status, int? RentalId);

    public enum RentalStartStatus
    {
        Started,
        CarNotFound,
        CarUnavailable
    }

    public enum FinishRentalResult
    {
        Finished,
        RentalNotFound
    }

    public enum MarkInvoicePaidResult
    {
        Paid,
        InvoiceNotFound,
        AlreadyPaid
    }

    public enum CarCreateResult
    {
        Created,
        RegistrationAlreadyExists
    }
}
