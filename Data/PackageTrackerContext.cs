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

    }
}