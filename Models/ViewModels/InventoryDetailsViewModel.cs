using InventoryManager.Models.Domain;

namespace InventoryManager.Models.ViewModels
{
    // ViewModel for the Inventory Details page
    // We use a ViewModel to pass exactly what the view needs, keeping the domain model clean.
    // It also allows us to include extra data like 'IsOwner' without modifying the entity itself.
    public class InventoryDetailsViewModel
    {
        // The inventory item to display
        public Inventory Inventory { get; set; }

        // Flag to indicate if the current logged-in user is the owner of this inventory
        // Used to show/hide the Settings tab
        public bool IsOwner { get; set; }
    }
}
