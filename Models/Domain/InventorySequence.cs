namespace InventoryManager.Models.Domain
{
    // Stores the current sequence counter for a specific inventory.
    // Used by the Sequence segment type in the custom-ID builder.
    public class InventorySequence
    {
        public Guid InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;

        // Monotonically increasing counter; incremented on each new item creation
        public long CurrentValue { get; set; } = 0;
    }
}
