 using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string PhoneNumber { get; set; }
        public string LicenceNumber { get; set; }

         public Tank Tank { get; set; }

        public ICollection<Request> Requests { get; set; }  
    }
}