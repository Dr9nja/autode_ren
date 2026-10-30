using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KooliProjekt.Application.Data
{
    [Index(nameof(RegistrationNumber), IsUnique = true)]
    public class RentalCar
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string RegistrationNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; }

        [Required]
        [StringLength(60)]
        public string Type { get; set; }

        [Range(typeof(decimal), "0.01", "10000")]
        public decimal TimeRatePerMinute { get; set; }

        [Range(typeof(decimal), "0", "10000")]
        public decimal KilometerRate { get; set; }
    }
}
