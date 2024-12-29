using System;

namespace PackageTracker.Models
{
    public class PackageTracking
    {
        public string Id { get; set; } // Represents the tracking number and acts as the primary key
        public string Carrier { get; set; }
        public string Status { get; set; }
        public DateTime ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
    }
}

