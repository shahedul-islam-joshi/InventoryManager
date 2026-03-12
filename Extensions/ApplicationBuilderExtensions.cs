using InventoryManager.Middleware;

namespace InventoryManager.Extensions
{
    // Centralised middleware registration to keep Program.cs clean.
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<BlockedUserMiddleware>();
            app.UseMiddleware<RequestCultureMiddleware>();
            return app;
        }
    }
}
