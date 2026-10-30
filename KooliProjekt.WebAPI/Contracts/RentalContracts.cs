using System;
using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.WebAPI.Contracts
{
    /// <summary>Credentials and profile details for creating a customer account.</summary>
    public sealed record RegisterRequest
    {
        [Required, StringLength(100)]
        public required string Name { get; init; }

        [Required, EmailAddress, StringLength(254)]
        public required string Email { get; init; }

        [Required, MinLength(12), StringLength(100)]
        public required string Password { get; init; }
    }

    /// <summary>Credentials for an existing customer account.</summary>
    public sealed record LoginRequest
    {
        [Required, EmailAddress, StringLength(254)]
        public required string Email { get; init; }

        [Required, StringLength(100)]
        public required string Password { get; init; }
    }

    /// <summary>Public account details for the signed-in customer.</summary>
    public sealed record AccountResponse(int Id, string Name, string Email, bool IsAdmin);

    /// <summary>A rentable car and its current per-minute and per-kilometer prices.</summary>
    public sealed record CarResponse(
        int Id,
        string RegistrationNumber,
        string Model,
        string Type,
        decimal TimeRatePerMinute,
        decimal KilometerRate);

    /// <summary>Adds a car with its registration, type, and rental tariffs.</summary>
    public sealed record CreateCarRequest
    {
        [Required, StringLength(20)]
        public required string RegistrationNumber { get; init; }

        [Required, StringLength(100)]
        public required string Model { get; init; }

        [Required, StringLength(60)]
        public required string Type { get; init; }

        [Range(typeof(decimal), "0.01", "10000")]
        public required decimal TimeRatePerMinute { get; init; }

        [Range(typeof(decimal), "0", "10000")]
        public required decimal KilometerRate { get; init; }
    }

    /// <summary>Starts a rental for the selected available car.</summary>
    public sealed record StartRentalRequest
    {
        [Range(1, int.MaxValue)]
        public required int CarId { get; init; }
    }

    /// <summary>Distance driven before ending a rental.</summary>
    public sealed record FinishRentalRequest
    {
        [Range(typeof(decimal), "0", "1000000")]
        public required decimal KilometersDriven { get; init; }
    }

    /// <summary>A rental and its invoice, when the rental is complete.</summary>
    public sealed record RentalResponse(
        int Id,
        string Status,
        DateTimeOffset StartedAt,
        DateTimeOffset? EndedAt,
        decimal? KilometersDriven,
        CarResponse Car,
        InvoiceResponse Invoice);

    /// <summary>An invoice created when a rental ends.</summary>
    public sealed record InvoiceResponse(
        int Id,
        DateTimeOffset CreatedAt,
        decimal Total,
        bool IsPaid,
        DateTimeOffset? PaidAt,
        InvoiceLineResponse Line);

    /// <summary>The rental charge represented by one invoice line.</summary>
    public sealed record InvoiceLineResponse(string Description, decimal Amount);

    /// <summary>An invoice displayed to an administrator.</summary>
    public sealed record AdminInvoiceResponse(
        int Id,
        string CustomerName,
        string CustomerEmail,
        string RegistrationNumber,
        string Description,
        decimal Total,
        bool IsPaid,
        DateTimeOffset CreatedAt,
        DateTimeOffset? PaidAt);

    /// <summary>Car data shown in the administrator's fleet list.</summary>
    public sealed record AdminCarResponse(
        int Id,
        string RegistrationNumber,
        string Model,
        string Type,
        decimal TimeRatePerMinute,
        decimal KilometerRate,
        bool IsAvailable);
}
