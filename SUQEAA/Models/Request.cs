using WebApplication3.Models; // تأكد من أن مساحة الاسم هذه مطابقة لمشروعك
using System;
using System.Collections.Generic; // لضمان توفر ICollection إذا لزم الأمر في نماذج أخرى

namespace WebApplication3.Models // <<<<<<<<<<< قوس فتح لمساحة الاسم
{
    public enum RequestStatus // <<<<<<<<<<< تعريف enum RequestStatus
    {
        Pending = 0,
        InDelivery = 1,
        Delivered = 2,
        Cancelled = 3
    } // <<<<<<<<<<< قوس إغلاق لـ enum RequestStatus

    public class Request // <<<<<<<<<<< تعريف الكلاس Request
    {
        public int Id { get; set; }

        public int Quantity { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? DeliveryDate { get; set; } // خاصية قابلة للقيم الفارغة (NULL)

        public RequestStatus Status { get; set; } // استخدام enum RequestStatus

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } // خاصية الملاحة (navigation property) للعميل

        public int TankId { get; set; }
        public Tank Tank { get; set; } // خاصية الملاحة للخزان

        public int DriverId { get; set; } // <<<<<<<<<<< هذه الخاصية هي int (غير قابلة للقيم الفارغة)
        public Driver Driver { get; set; } // خاصية الملاحة للسائق

        // **هام:** تم التأكد من حذف أي أسطر زائدة أو غير صحيحة هنا، مثل:
        // ICollection<Request> Requests = new List<Request>();
    } // <<<<<<<<<<< قوس إغلاق للكلاس Request

} // <<<<<<<<<<< قوس إغلاق لمساحة الاسم WebApplication3.Models