using Microsoft.EntityFrameworkCore;
using PackageTracker.Models;
using System;
using System.Linq;

namespace PackageTracker.Data
{
    public class PackageTrackerContext : DbContext
    {
        public PackageTrackerContext(DbContextOptions<PackageTrackerContext> options) : base(options) { }

        public DbSet<PackageTracking> PackageTrackings { get; set; }

        public double? GetAverageShippingTime(string origin, string destination) 
        {
            var relevantPackages = PackageTrackings
                .Where(p => p.Origin == origin && p.Destination == destination)
                .ToList();

            if (!relevantPackages.Any()) return null;

            var totalDays = relevantPackages
                .Where(p => p.DeliveryDate > p.ShippingDate)
                .Sum(p => (p.DeliveryDate - p.ShippingDate).TotalDays);

            return totalDays / relevantPackages.Count;
        }

    }
}