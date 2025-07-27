using WebApplication3.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Region
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<District> Districts { get; set; }
        public ICollection<Customer> Customers { get; set; } // ← أضف هذا
    }
}