using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.Domain
{
    public class Inventory
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // Optional cover image URL (uploaded to cloud storage or local wwwroot)
        public string? ImageUrl { get; set; }

        // OwnerId is nullable to allow form binding before the value is set in the controller
        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }

        // Optimistic concurrency token
        [Timestamp]
        public byte[]? Version { get; set; }

        // Navigation properties
        public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<InventoryField> Fields { get; set; } = new List<InventoryField>();
        public ICollection<IdElement> IdElements { get; set; } = new List<IdElement>();
        public ICollection<InventoryAccess> InventoryAccesses { get; set; } = new List<InventoryAccess>();

        // One-to-one relationship with the sequence counter
        public InventorySequence? Sequence { get; set; }
    }
}
