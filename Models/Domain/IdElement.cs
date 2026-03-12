namespace InventoryManager.Models.Domain
{
    // Defines one segment of the custom-ID template for an inventory.
    // The full custom ID is built by concatenating all IdElements ordered by Order.
    public class IdElement
    {
        public int Id { get; set; }

        public Guid InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;

        // Segment type: "Random", "Fixed", "Sequence", "Date", "Text"
        public string Type { get; set; } = "Fixed";

        // Display order of this segment within the custom ID
        public int Order { get; set; }

        // For "Fixed" type: the literal text
        // For "Random": character set, length etc. encoded as JSON
        // For "Sequence": padding/prefix format
        public string? Format { get; set; }
    }
}
