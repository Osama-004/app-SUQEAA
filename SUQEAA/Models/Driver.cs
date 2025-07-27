// في WebApplication3.Models/Driver.cs
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

        // خاصية الملاحة للعلاقة واحد-إلى-واحد: السائق مسؤول عن خزان واحد فقط
        public Tank Tank { get; set; }

        public ICollection<Request> Requests { get; set; } // السائق لا يزال يمكن أن يكون له عدة طلبات
    }
}