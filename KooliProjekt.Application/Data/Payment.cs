using System;
using System.ComponentModel.DataAnnotations;

namespace KooliProjekt.Application.Data
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public CarOrder Order { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        public DateTime OrderDate { get; set; }
        public bool GetToHome { get; set; }
    }
}
