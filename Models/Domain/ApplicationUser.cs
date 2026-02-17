using Microsoft.AspNetCore.Identity;

namespace InventoryManager.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        // We add only simple properties today

        public bool IsBlocked { get; set; } = false;

        public string? Theme { get; set; }

        public string? Language { get; set; }
    }
}
