using InventoryManager.Models.Domain;
using InventoryManager.Models.DTOs;

namespace InventoryManager.Models.ViewModels
{
    // ViewModel for the Inventory Details page.
    // Using a ViewModel lets us pass computed flags (IsOwner, CanEdit) and related
    // collections to the view without polluting the domain model with UI concerns.
    public class InventoryDetailsViewModel
    {
        // The inventory being displayed
        public Inventory Inventory { get; set; } = null!;

        // True if the current user is the inventory owner.
        // Used to show/hide the Access tab and owner-only controls.
        public bool IsOwner { get; set; }

        // True if the current user can create, edit, or delete items.
        // This is true for both the owner AND users with explicitly granted access.
        // WHY SEPARATE FROM IsOwner?
        // The view needs two different checks:
        //   - IsOwner  → show the Access management tab (owner-only feature)
        //   - CanEdit  → show Add Item / Delete buttons (owner + granted users)
        public bool CanEdit { get; set; }

        // Items belonging to this inventory, ordered newest-first
        public List<Item> Items { get; set; } = new List<Item>();

        // Users who have been explicitly granted write access (excluding the owner).
        // Only populated when IsOwner is true — used to render the Access tab.
        public List<ApplicationUser> UsersWithAccess { get; set; } = new List<ApplicationUser>();

        // Discussion posts pre-loaded for server-side rendering on initial page load.
        // Real-time updates are handled by SignalR after the page loads.
        public List<DiscussionPostDto> DiscussionPosts { get; set; } = new List<DiscussionPostDto>();
    }
}

