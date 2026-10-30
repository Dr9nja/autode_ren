using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class Address
    {
        public int Id { get; set; }

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

        public Customer Customer { get; set; }
    }
}
