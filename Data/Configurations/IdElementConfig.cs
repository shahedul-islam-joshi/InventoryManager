using InventoryManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Data.Configurations
{
    public class IdElementConfig : IEntityTypeConfiguration<IdElement>
    {
        public void Configure(EntityTypeBuilder<IdElement> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Type).IsRequired().HasMaxLength(30);
            builder.Property(e => e.Format).HasMaxLength(500);

            builder.HasOne(e => e.Inventory)
                   .WithMany(i => i.IdElements)
                   .HasForeignKey(e => e.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
