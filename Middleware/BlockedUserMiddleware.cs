using InventoryManager.Models.Domain;
using Microsoft.AspNetCore.Identity;

namespace InventoryManager.Middleware
{
    public class BlockedUserMiddleware
    {
        private readonly RequestDelegate _next;

        public BlockedUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            // 1. Check if the user is logged in
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // 2. SAFETY: Don't run this logic if they are already trying to Logout or are on the Login page
                // This prevents infinite redirect loops.
                var path = context.Request.Path.Value?.ToLower();
                if (path != null && !path.Contains("/identity/account/logout") && !path.Contains("/identity/account/login"))
                {
                    var user = await userManager.GetUserAsync(context.User);

                    if (user != null && user.IsBlocked)
                    {
                        // 3. Log them out and send them away
                        await signInManager.SignOutAsync();
                        context.Response.Redirect("/Identity/Account/Login?error=blocked");
                        return; // Stop the request here
                    }
                }
            }

            // Continue to the next piece of middleware (Authorization, etc.)
            await _next(context);
        }
    }
}