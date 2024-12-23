using Microsoft.EntityFrameworkCore;
using PackageTracker.Models;

namespace PackageTracker.Data
{
    public class PackageTrackerContext : DbContext
    {
        public PackageTrackerContext(DbContextOptions<PackageTrackerContext> options) : base(options) { }

        public DbSet<PackageTracking> PackageTrackings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PackageTracking>()
                .HasKey(p => p.Id); // Ensures Id is the primary key
        }

    }
}