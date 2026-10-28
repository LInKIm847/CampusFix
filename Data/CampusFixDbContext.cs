
using CampusFix.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Data
{
    // Includes ASP.NET Core Identity tables and CampusFix tables.
    public class CampusFixDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public CampusFixDbContext(
            DbContextOptions<CampusFixDbContext> options)
            : base(options)
        {
        }

        public DbSet<MaintenanceRequest> MaintenanceRequests
            => Set<MaintenanceRequest>();

        public DbSet<Category> Categories
            => Set<Category>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configure the standard Identity tables first.
            base.OnModelCreating(builder);

            // Store enum values as text, e.g. "Submitted" and "High".
            builder.Entity<MaintenanceRequest>()
                .Property(r => r.Status)
                .HasConversion<string>();

            builder.Entity<MaintenanceRequest>()
                .Property(r => r.Priority)
                .HasConversion<string>();

            // One user can submit multiple maintenance requests.
            builder.Entity<MaintenanceRequest>()
                .HasOne(r => r.User)
                .WithMany(u => u.MaintenanceRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One category can be used by multiple requests.
            builder.Entity<MaintenanceRequest>()
                .HasOne(r => r.Category)
                .WithMany(c => c.MaintenanceRequests)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
