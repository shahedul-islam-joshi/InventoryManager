using InventoryManager.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManager.Data.Configurations
{
    public class ItemLikeConfig : IEntityTypeConfiguration<ItemLike>
    {
        public void Configure(EntityTypeBuilder<ItemLike> builder)
        {
            builder.HasKey(l => l.Id);

            // A user can like each item at most once
            builder.HasIndex(l => new { l.ItemId, l.UserId }).IsUnique();

            builder.HasOne(l => l.Item)
                   .WithMany(i => i.ItemLikes)
                   .HasForeignKey(l => l.ItemId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
