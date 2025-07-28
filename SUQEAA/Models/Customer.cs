using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models  
{
    public class Customer
    {
        public int Id { get; set; }
        public string Email { get; set; }  

        [Required]
        public string Name { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public int RegionId { get; set; }
        public Region Region { get; set; }

        public int DistrictId { get; set; }
        public District District { get; set; }

         public int SimpleUserId { get; set; }  
        public SimpleUser SimpleUser { get; set; }  

        public ICollection<Request> Requests { get; set; }
    }
}