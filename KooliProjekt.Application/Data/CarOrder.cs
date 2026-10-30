using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class CarOrder
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        [StringLength(100)]
        public string CarModel { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        public DateTime OrderDate { get; set; }
        public bool GetToHome { get; set; }
        public Payment Payment { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
