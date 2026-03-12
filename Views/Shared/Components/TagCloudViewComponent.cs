using InventoryManager.Data;
using InventoryManager.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Views.Shared.Components
{
    // ViewComponent that renders the tag cloud on the Home page sidebar.
    // Encapsulated as a ViewComponent so it can be reused on any page with @await Component.InvokeAsync("TagCloud")
    public class TagCloudViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public TagCloudViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int maxTags = 30)
        {
            var tags = await _context.Tags
                .Select(t => new { Tag = t, Count = t.InventoryTags.Count })
                .Where(x => x.Count > 0)
                .OrderByDescending(x => x.Count)
                .Take(maxTags)
                .ToListAsync();

            var result = tags.Select(x => (x.Tag, x.Count)).ToList();
            return View(result);
        }
    }
}
