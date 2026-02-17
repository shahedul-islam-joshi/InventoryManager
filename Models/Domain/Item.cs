using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.Domain
{
    // Represents a single item within an inventory
    public class Item
    {
        public Guid Id { get; set; }

        // Foreign key to the Inventory this item belongs to
        public Guid InventoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
