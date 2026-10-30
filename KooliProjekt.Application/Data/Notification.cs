using System;
using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class Notification
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public CarOrder Order { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        public DateTime ReceiveDate { get; set; }
        public bool GetToHome { get; set; }
    }
}
