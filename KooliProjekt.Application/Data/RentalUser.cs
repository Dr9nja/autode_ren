using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.Application.Data
{
    [Index(nameof(NormalizedEmail), IsUnique = true)]
    public class RentalUser
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        [Required]
        [StringLength(254)]
        public string NormalizedEmail { get; set; }

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; }

        public bool IsAdmin { get; set; }
    }
}
