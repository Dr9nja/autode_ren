using System;
using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class Rental
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public RentalCar Car { get; set; }
        public int UserId { get; set; }
        public RentalUser User { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? EndedAt { get; set; }
        public decimal? KilometersDriven { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public Invoice Invoice { get; set; }
    }
}
