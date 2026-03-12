using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InventoryManager.Models.Domain;
using InventoryManager.Data.Configurations;

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
        public DbSet<ItemLike> ItemLikes { get; set; }

        // Stores explicit write-access grants for non-owner users
        public DbSet<InventoryAccess> InventoryAccesses { get; set; }

        // Stores discussion messages for each inventory
        public DbSet<DiscussionPost> DiscussionPosts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<InventoryTag> InventoryTags { get; set; }
        public DbSet<InventoryField> InventoryFields { get; set; }
        public DbSet<IdElement> IdElements { get; set; }
        public DbSet<InventorySequence> InventorySequences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration<T> classes in this assembly automatically.
            // This keeps OnModelCreating clean — each entity's mapping is in its own file.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
