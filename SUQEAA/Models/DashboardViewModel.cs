 namespace WebApplication3.Models  
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public double TotalAvailableTankCapacity { get; set; }
        public string MostRequestedAreaName { get; set; }  
        public int MostRequestedAreaCount { get; set; }  
 
    }
}