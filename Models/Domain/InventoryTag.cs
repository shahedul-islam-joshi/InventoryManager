namespace InventoryManager.Models.Domain
{
    // Join table — many Inventories can share many Tags
    public class InventoryTag
    {
        public Guid InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
