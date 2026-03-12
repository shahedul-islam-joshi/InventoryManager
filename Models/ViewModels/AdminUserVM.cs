namespace InventoryManager.Models.ViewModels
{
    // Used to display a single user row in the Admin panel.
    public class AdminUserVM
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }
    }
}
