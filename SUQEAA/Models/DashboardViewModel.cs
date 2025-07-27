// في مجلد Models الخاص بك
namespace WebApplication3.Models // تأكد من مساحة الاسم الصحيحة
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public double TotalAvailableTankCapacity { get; set; }
        public string MostRequestedAreaName { get; set; } // تتطلب استعلامًا معقدًا
        public int MostRequestedAreaCount { get; set; } // تتطلب استعلامًا معقدًا
        // أضف أي خصائص إحصائية أخرى تحتاجها
    }
}