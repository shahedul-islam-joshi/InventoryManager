using InventoryManager.Services;
using InventoryManager.Services.Interfaces;

namespace InventoryManager.Extensions
{
    // Centralises all service registrations so Program.cs stays clean.
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAccessService, AccessService>();
            services.AddScoped<IDiscussionService, DiscussionService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ICustomIdService, CustomIdService>();

            return services;
        }
    }
}
