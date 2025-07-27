using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models // تأكد من أن مساحة الاسم هذه مطابقة لمشروعك
{
    public class Customer
    {
        public int Id { get; set; }
        public string Email { get; set; } // تأكد من وجود هذا لتسهيل البحث بالبريد الإلكتروني

        [Required]
        public string Name { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public int RegionId { get; set; }
        public Region Region { get; set; }

        public int DistrictId { get; set; }
        public District District { get; set; }

        // <<<<<<<<<<< الخصائص الجديدة لربط العميل بـ SimpleUser
        public int SimpleUserId { get; set; } // مفتاح خارجي لربط Customer بـ SimpleUser
        public SimpleUser SimpleUser { get; set; } // خاصية الملاحة

        public ICollection<Request> Requests { get; set; }
    }
}