 
using System.Collections.Generic;

namespace WebApplication3.Models 
{
    public class DistributorDashboardViewModel
    {
        public List<Request> PendingRequests { get; set; }  
        public List<Request> CompletedRequests { get; set; }  

        public int TotalRequestsCount { get; set; }
        public int PendingRequestsCount { get; set; }  

    
    }
}