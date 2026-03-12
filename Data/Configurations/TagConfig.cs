using InventoryManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Data.Configurations
{
    public class TagConfig : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t => t.Id);

            // Tag names are case-insensitively unique — normalised to lowercase in service layer
            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
            builder.HasIndex(t => t.Name).IsUnique();

            // Configure the many-to-many join table
            builder.HasMany(t => t.InventoryTags)
                   .WithOne(it => it.Tag)
                   .HasForeignKey(it => it.TagId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class InventoryTagConfig : IEntityTypeConfiguration<InventoryTag>
    {
        public void Configure(EntityTypeBuilder<InventoryTag> builder)
        {
            // Composite primary key
            builder.HasKey(it => new { it.InventoryId, it.TagId });

            builder.HasOne(it => it.Inventory)
                   .WithMany(i => i.InventoryTags)
                   .HasForeignKey(it => it.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
