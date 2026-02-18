using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InventoryManager.Models.Domain;

namespace InventoryManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Item> Items { get; set; }

        // Stores explicit write-access grants for non-owner users
        public DbSet<InventoryAccess> InventoryAccesses { get; set; }

        // Stores discussion messages for each inventory
        public DbSet<DiscussionPost> DiscussionPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configuration for InventoryAccess
            // WHY FLUENT API?
            // A unique composite index cannot be expressed with a single data annotation.
            // Fluent API keeps the domain model free of EF-specific attributes.
            modelBuilder.Entity<InventoryAccess>(entity =>
            {
                // Prevent the same user from being granted access to the same inventory twice
                entity.HasIndex(a => new { a.InventoryId, a.UserId }).IsUnique();
            });
        }
    }
}

