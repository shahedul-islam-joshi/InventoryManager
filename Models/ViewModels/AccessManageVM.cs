using InventoryManager.Models.Domain;

namespace InventoryManager.Models.ViewModels
{
    // AccessManageVM is used by the _AccessTab partial view.
    // It bundles everything the view needs so the partial stays simple and testable.
    public class AccessManageVM
    {
        // The inventory whose access list is being managed
        public Guid InventoryId { get; set; }

        // Users who currently have write access (excluding the owner)
        public List<ApplicationUser> UsersWithAccess { get; set; } = new List<ApplicationUser>();

        // Email address typed into the "Grant Access" form
        // Not required at the model level — validation is done in the controller
        public string? NewUserEmail { get; set; }
    }
}
