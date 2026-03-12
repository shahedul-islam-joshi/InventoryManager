using InventoryManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Data.Configurations
{
    public class InventorySequenceConfig : IEntityTypeConfiguration<InventorySequence>
    {
        public void Configure(EntityTypeBuilder<InventorySequence> builder)
        {
            // Primary Key
            builder.HasKey(s => s.InventoryId);

            // One-to-One relationship to Inventory
            builder.HasOne(s => s.Inventory)
                .WithOne(i => i.Sequence)
                .HasForeignKey<InventorySequence>(s => s.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
