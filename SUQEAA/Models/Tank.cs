// في WebApplication3.Models/Tank.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models // تأكد من مساحة الاسم الصحيحة
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

        // خاصية المفتاح الخارجي والسائق للعلاقة واحد-إلى-واحد
        public int DriverId { get; set; } // السائق المسؤول عن هذا الخزان
        public Driver Driver { get; set; } // خاصية الملاحة

        public ICollection<Request> Requests { get; set; }
    }
}