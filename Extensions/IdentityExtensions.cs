using Microsoft.AspNetCore.Identity;
using InventoryManager.Models.Domain;

namespace InventoryManager.Extensions
{
    // Identity-related extension helpers.
    public static class IdentityExtensions
    {
        // Convenience wrapper: returns true if the user is in the Admin role.
        public static async Task<bool> IsAdminAsync(this UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            return await userManager.IsInRoleAsync(user, "Admin");
        }

        // Configures sensible Identity password / sign-in options.
        public static IdentityOptions ApplyApplicationDefaults(this IdentityOptions options)
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            return options;
        }
    }
}
