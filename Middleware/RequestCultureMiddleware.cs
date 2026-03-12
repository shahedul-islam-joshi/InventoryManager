using InventoryManager.Helpers;
using InventoryManager.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;

namespace InventoryManager.Middleware
{
    // Sets the request culture based on the authenticated user's Language preference.
    // Falls back to the OS/browser culture if the user has no preference set.
    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null && !string.IsNullOrWhiteSpace(user.Language))
                {
                    var culture = LocalizationHelper.Resolve(user.Language);
                    var cultureInfo = new System.Globalization.CultureInfo(culture);

                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }
            }

            await _next(context);
        }
    }
}
