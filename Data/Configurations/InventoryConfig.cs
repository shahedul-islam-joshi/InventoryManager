using InventoryManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Data.Configurations
{
    public class InventoryConfig : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Title).IsRequired().HasMaxLength(200);
            builder.Property(i => i.Description).HasMaxLength(2000);
            builder.Property(i => i.Category).HasMaxLength(100);
            builder.Property(i => i.ImageUrl).HasMaxLength(500);

            // Optimistic concurrency token
            builder.Property(i => i.Version).IsRowVersion();

            // Owner relationship — restrict delete so orphaned inventories stay readable
            builder.HasOne(i => i.Owner)
                   .WithMany()
                   .HasForeignKey(i => i.OwnerId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Index on OwnerId for fast "my inventories" queries
            builder.HasIndex(i => i.OwnerId);
        }
    }
}
