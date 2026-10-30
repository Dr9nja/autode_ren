using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class Shop
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Street { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }
    }
}
