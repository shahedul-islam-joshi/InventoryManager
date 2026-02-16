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
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null && user.IsBlocked)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Identity/Account/Login?error=blocked");
                    return;
                }
            }

            await _next(context);
        }
    }
}
