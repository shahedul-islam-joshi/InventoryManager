using InventoryManager.Models.Domain;

namespace InventoryManager.Services.Interfaces
{
    // IAccessService defines the contract for all inventory write-access logic.
    //
    // WHY AN INTERFACE?
    // Controllers depend on this abstraction, not the concrete class.
    // This makes the service swappable and testable without touching controllers.
    public interface IAccessService
    {
        // Returns true if the user can edit the inventory itself (e.g., settings).
        // Owner is always allowed. Users in InventoryAccess table are also allowed.
        bool CanEditInventory(Guid inventoryId, string userId);

        // Returns true if the user can create, edit, or delete items inside an inventory.
        // Uses the same rules as CanEditInventory — kept separate so they can diverge later.
        bool CanEditItems(Guid inventoryId, string userId);

        // Grants write access to the user identified by their email address.
        // Throws if the user is not found or already has access.
        Task GrantAccessAsync(Guid inventoryId, string userEmail);

        // Removes write access for the given userId from the inventory.
        Task RemoveAccessAsync(Guid inventoryId, string userId);

        // Returns all users (excluding the owner) who have been granted write access.
        Task<List<ApplicationUser>> GetUsersWithAccessAsync(Guid inventoryId);
    }
}
