using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class Customer
    {
        public int Id { get; set; }

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

        public int AddressId { get; set; }
        public Address Address { get; set; }
        public ICollection<CarOrder> Orders { get; set; } = new List<CarOrder>();
    }
}
