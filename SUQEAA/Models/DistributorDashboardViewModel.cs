// في مجلد Models الخاص بك
// DistributorDashboardViewModel.cs
using System.Collections.Generic;

namespace WebApplication3.Models // تأكد من مساحة الاسم الصحيحة
{
    public class DistributorDashboardViewModel
    {
        public List<Request> PendingRequests { get; set; } // الطلبات قيد التوصيل/المعلقة
        public List<Request> CompletedRequests { get; set; } // الطلبات التي تم توصيلها

        public int TotalRequestsCount { get; set; }
        public int PendingRequestsCount { get; set; } // عدد الطلبات قيد التوصيل

        // يمكنك إبقاء Drivers هنا إذا كنت تريد قائمة بكل السائقين
        // public List<Driver> Drivers { get; set; }
        // أضف أي خصائص أخرى يحتاجها الموزع في لوحة التحكم
    }
}