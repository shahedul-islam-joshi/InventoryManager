using InventoryManager.Data;
using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;
using InventoryManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventoryManager.Controllers
{
    // WHY CONTROLLERS STAY THIN:
    // ItemController handles HTTP routing and model binding only.
    // All permission decisions are delegated to IAccessService.
    // This means the "who can edit items" rule lives in exactly one place.
    [Authorize]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccessService _accessService;

        public ItemController(ApplicationDbContext context, IAccessService accessService)
        {
            _context = context;
            _accessService = accessService;
        }

        // GET: Item/Create?inventoryId=...
        // Displays the form to create a new Item for a specific Inventory
        [HttpGet]
        public IActionResult Create(Guid inventoryId)
        {
            var model = new ItemCreateViewModel
            {
                InventoryId = inventoryId
            };
            return View(model);
        }

        // POST: Item/Create
        // Handles the submission of the Create Item form.
        // Permission check: user must be owner OR have been granted write access.
        [HttpPost]
        public IActionResult Create(ItemCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // WHY USE AccessService HERE?
                // Previously this controller checked inventory.OwnerId == userId directly.
                // That approach breaks as soon as we add access grants — it would always
                // deny granted users. AccessService encapsulates the full rule:
                //   "owner OR explicitly granted user → allowed".
                if (!_accessService.CanEditItems(model.InventoryId, userId))
                {
                    // Return 403 Forbidden — the user is authenticated but not authorised
                    return Forbid();
                }

                var newItem = new Item
                {
                    Id = Guid.NewGuid(),
                    InventoryId = model.InventoryId,
                    Name = model.Name,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Items.Add(newItem);
                _context.SaveChanges();

                return RedirectToAction("Details", "Inventory", new { id = model.InventoryId });
            }

            return View(model);
        }

        // POST: Item/Delete/5
        // Deletes an item if the current user has write access to the parent inventory.
        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            var item = _context.Items.FirstOrDefault(i => i.Id == id);

            if (item == null)
                return RedirectToAction("Index", "Inventory");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // WHY USE AccessService HERE?
            // Same reason as Create: the old direct OwnerId check would deny granted users.
            // AccessService is the single source of truth for "can this user edit items".
            if (!_accessService.CanEditItems(item.InventoryId, userId))
            {
                // Return 403 Forbidden — authenticated but not authorised
                return Forbid();
            }

            _context.Items.Remove(item);
            _context.SaveChanges();

            return RedirectToAction("Details", "Inventory", new { id = item.InventoryId });
        }

        // POST: Item/ToggleLike
        // Toggles a like for an item for the current logged-in user.
        // Returns the updated like count as JSON.
        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] LikeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var item = await _context.Items.Include(i => i.ItemLikes).FirstOrDefaultAsync(i => i.Id == request.ItemId);

            if (item == null) return NotFound();

            var existingLike = item.ItemLikes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike == null)
            {
                // Like it!
                item.ItemLikes.Add(new ItemLike
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    UserId = userId
                });
            }
            else
            {
                // Unlike it!
                _context.ItemLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();

            return Json(new { likes = item.ItemLikes.Count });
        }
    }

    public class LikeRequest
    {
        public Guid ItemId { get; set; }
    }
}
