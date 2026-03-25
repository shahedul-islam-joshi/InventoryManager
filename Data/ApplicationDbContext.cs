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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1. Keep the base call for Identity tables!
            base.OnModelCreating(builder);

            // 2. FIX: InventoryTag Primary Key (Composite Key)
            builder.Entity<InventoryTag>()
                .HasKey(it => new { it.InventoryId, it.TagId });

            // 3. FIX: InventorySequence (Ignore it if it's just for custom IDs)
            builder.Ignore<InventorySequence>();

            // 4. ENABLE: Cascade Delete for Users -> Inventories
            builder.Entity<Inventory>()
                .HasOne(i => i.Owner)
                .WithMany(u => u.Inventories)
                .HasForeignKey(i => i.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. ENABLE: Cascade Delete for Inventories -> Items
            builder.Entity<Item>()
                .HasOne(i => i.Inventory)
                .WithMany(inv => inv.Items)
                .HasForeignKey(i => i.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
