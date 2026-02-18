using InventoryManager.Models.Domain;

namespace InventoryManager.Helpers
{
    // PermissionHelper provides reusable, documented permission checks.
    //
    // WHY A HELPER CLASS?
    // The ownership check (inventory.OwnerId == userId) is a simple string comparison,
    // but it is used in multiple places (AccessService, controllers).
    // Centralising it here avoids duplication and makes the intent explicit.
    public static class PermissionHelper
    {
        // Returns true if the given userId matches the inventory's OwnerId.
        // The owner always has full permission — this is the single source of that rule.
        public static bool IsOwner(Inventory inventory, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            return inventory.OwnerId == userId;
        }
    }
}
