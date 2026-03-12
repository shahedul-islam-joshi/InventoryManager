namespace InventoryManager.Models.Domain
{
    // Defines a custom field displayed in an inventory's Items table.
    // Each field maps to one of the fixed Item slots (Text1..3, Number1..3, Bool1..3, Date1..3).
    public class InventoryField
    {
        public int Id { get; set; }

        public Guid InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;

        // FieldType: "Text", "Number", "Bool", "Date"
        public string FieldType { get; set; } = "Text";

        // Slot index 1–3 within the type group (e.g., Text slot 2)
        public int SlotIndex { get; set; }

        // Label shown as the column header
        public string Title { get; set; } = string.Empty;

        // Optional longer description / tooltip text
        public string? Description { get; set; }

        // Whether this field column is shown in the items table
        public bool ShowInTable { get; set; } = true;

        // Display order in the table (lower = leftmost)
        public int Order { get; set; }
    }
}
