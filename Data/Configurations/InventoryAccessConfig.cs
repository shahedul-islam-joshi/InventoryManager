using InventoryManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Data.Configurations
{
    public class InventoryAccessConfig : IEntityTypeConfiguration<InventoryAccess>
    {
        public void Configure(EntityTypeBuilder<InventoryAccess> builder)
        {
            builder.HasKey(a => a.Id);

            // Prevent the same user from being granted access to the same inventory twice
            builder.HasIndex(a => new { a.InventoryId, a.UserId }).IsUnique();

            builder.HasOne(a => a.Inventory)
                   .WithMany(i => i.InventoryAccesses)
                   .HasForeignKey(a => a.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
