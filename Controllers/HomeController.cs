using InventoryManager.Data;
using InventoryManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITagService _tagService;

        public HomeController(ApplicationDbContext context, ITagService tagService)
        {
            _context = context;
            _tagService = tagService;
        }

        // Shows the landing page with the 5 most recently updated inventories and a tag cloud.
        // Public — no [Authorize].
        public async Task<IActionResult> Index()
        {
            // Latest 5 public inventories (all inventories for logged-in users)
            var query = User.Identity?.IsAuthenticated == true
                ? _context.Inventories.AsQueryable()
                : _context.Inventories.Where(i => i.IsPublic);

            var latest = await query
                .Include(i => i.InventoryTags).ThenInclude(it => it.Tag)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync();

            var tagCloud = await _tagService.GetTagCloudAsync();

            ViewBag.TagCloud = tagCloud;
            return View(latest);
        }
    }
}
