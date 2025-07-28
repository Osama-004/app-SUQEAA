 using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models  
{
    public class Tank
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public double Capacity { get; set; }
        public double CurrentCapacity { get; set; }
        public string Location { get; set; }
        public double PricePerLiter { get; set; }

        public int RegionId { get; set; }
        public Region Region { get; set; }

        public int? DistrictId { get; set; }
        public District District { get; set; }

         
        public int DriverId { get; set; }  
        public Driver Driver { get; set; }  

        public ICollection<Request> Requests { get; set; }
    }
}