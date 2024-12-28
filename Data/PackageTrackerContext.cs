using Microsoft.EntityFrameworkCore;
using PackageTracker.Models;
using System;
using System.Linq;

namespace PackageTracker.Data
{
    public class PackageTrackerContext : DbContext
    {
        // Constructor to initialize database context with options
        public PackageTrackerContext(DbContextOptions<PackageTrackerContext> options) : base(options) { }

        // Represents the PackageTrackings table in the database
        public DbSet<PackageTracking> PackageTrackings { get; set; }

    }
}