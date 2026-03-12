using InventoryManager.Data;
using InventoryManager.Helpers;
using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventoryManager.Controllers
{
    // Profile page — shows the user's owned inventories and inventories they have write access to.
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Profile or /Profile/Index/{userId}
        // Shows the profile of the given user (or the current user if no id provided).
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? userId)
        {
            var targetId = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (targetId == null) return Challenge();

            var user = await _userManager.FindByIdAsync(targetId);
            if (user == null) return NotFound();

            var owned = await _context.Inventories
                .Where(i => i.OwnerId == targetId)
                .OrderByDescending(i => i.CreatedAt)
                .Include(i => i.InventoryTags).ThenInclude(it => it.Tag)
                .ToListAsync();

            // Inventories the user has explicit write access to (not owned by them)
            var sharedIds = await _context.InventoryAccesses
                .Where(a => a.UserId == targetId)
                .Select(a => a.InventoryId)
                .ToListAsync();

            var shared = await _context.Inventories
                .Where(i => sharedIds.Contains(i.Id))
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var vm = new ProfileVM
            {
                User = user,
                OwnedInventories = owned,
                SharedInventories = shared
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SetTheme(string theme)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.Theme = ThemeHelper.Resolve(theme);
                await _userManager.UpdateAsync(user);
            }
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }

        [HttpPost]
        public async Task<IActionResult> SetLanguage(string language)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.Language = LocalizationHelper.Resolve(language);
                await _userManager.UpdateAsync(user);
            }
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }
    }
}
