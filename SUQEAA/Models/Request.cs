using WebApplication3.Models;  
using System;
using System.Collections.Generic;  

namespace WebApplication3.Models 
{
    public enum RequestStatus  
    {
        Pending = 0,
        InDelivery = 1,
        Delivered = 2,
        Cancelled = 3
    }  

    public class Request  
    {
        public int Id { get; set; }

        public int Quantity { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? DeliveryDate { get; set; }  

        public RequestStatus Status { get; set; } 

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }  

        public int TankId { get; set; }
        public Tank Tank { get; set; }  

        public int DriverId { get; set; }  
        public Driver Driver { get; set; }  

 
    }  

}  