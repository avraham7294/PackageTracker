using Microsoft.EntityFrameworkCore;
using PackageTracker.Models;

namespace PackageTracker.Data
{
    public class PackageTrackerContext : DbContext
    {
        public PackageTrackerContext(DbContextOptions<PackageTrackerContext> options) : base(options) { }

        public DbSet<PackageTracking> Packages { get; set; }
    }
}
