using InventoryManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Data.Configurations
{
    public class InventoryFieldConfig : IEntityTypeConfiguration<InventoryField>
    {
        public void Configure(EntityTypeBuilder<InventoryField> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.FieldType).IsRequired().HasMaxLength(20);
            builder.Property(f => f.Title).IsRequired().HasMaxLength(100);
            builder.Property(f => f.Description).HasMaxLength(300);

            // SlotIndex must be 1, 2, or 3 — enforced at application layer; index for lookups
            builder.HasIndex(f => new { f.InventoryId, f.FieldType, f.SlotIndex }).IsUnique();

            builder.HasOne(f => f.Inventory)
                   .WithMany(i => i.Fields)
                   .HasForeignKey(f => f.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
