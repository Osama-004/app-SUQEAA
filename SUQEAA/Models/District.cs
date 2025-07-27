using WebApplication3.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class District
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int RegionId { get; set; }
        public Region Region { get; set; }

        public ICollection<Customer> Customers { get; set; }
    }
}
