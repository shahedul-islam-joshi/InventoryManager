using InventoryManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Data.Configurations
{
    public class ItemConfig : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
            builder.Property(i => i.CustomId).HasMaxLength(100);
            builder.Property(i => i.Description).HasMaxLength(2000);

            // Decimal slots with reasonable precision
            builder.Property(i => i.Number1).HasPrecision(18, 4);
            builder.Property(i => i.Number2).HasPrecision(18, 4);
            builder.Property(i => i.Number3).HasPrecision(18, 4);

            // Optimistic concurrency token
            builder.Property(i => i.Version).IsRowVersion();

            // Unique custom ID per inventory (if set)
            builder.HasIndex(i => new { i.InventoryId, i.CustomId }).IsUnique().HasFilter("\"CustomId\" IS NOT NULL");

            // Index for fast items-by-inventory queries
            builder.HasIndex(i => i.InventoryId);

            builder.HasOne(i => i.Inventory)
                   .WithMany(inv => inv.Items)
                   .HasForeignKey(i => i.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
