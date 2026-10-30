using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class InvoiceLine
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        public decimal Amount { get; set; }
    }
}
