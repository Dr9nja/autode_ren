using System;
using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class Invoice
    {
        public int Id { get; set; }
        public int RentalId { get; set; }
        public Rental Rental { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public decimal Total { get; set; }
        public bool IsPaid { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public InvoiceLine Line { get; set; }
    }
}
