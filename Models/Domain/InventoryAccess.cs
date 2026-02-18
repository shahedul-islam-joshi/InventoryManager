namespace InventoryManager.Models.Domain
{
    // Represents a write-access grant: one row = one user allowed to edit one inventory.
    // The Owner is NOT stored here — ownership is tracked by Inventory.OwnerId.
    // This table only holds explicitly granted access for non-owner users.
    public class InventoryAccess
    {
        public Guid Id { get; set; }

        // Foreign key: which inventory this access grant belongs to
        public Guid InventoryId { get; set; }

        // Foreign key: which user has been granted access (ASP.NET Identity user ID)
        public string UserId { get; set; } = string.Empty;

        // When the access was granted — useful for auditing
        public DateTime GrantedAt { get; set; }
    }
}
