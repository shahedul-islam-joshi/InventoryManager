using InventoryManager.Models.Domain;

namespace InventoryManager.Models.ViewModels
{
    public class ProfileVM
    {
        public ApplicationUser User { get; set; } = null!;

        // Inventories this user owns
        public List<Inventory> OwnedInventories { get; set; } = new();

        // Inventories where the user has been granted write access
        public List<Inventory> SharedInventories { get; set; } = new();
    }
}
