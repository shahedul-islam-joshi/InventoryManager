using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.Domain
{
    public class ItemLike
    {
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
