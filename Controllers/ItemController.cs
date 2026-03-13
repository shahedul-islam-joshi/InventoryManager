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

        // GET: Item/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (!_accessService.CanEditItems(item.InventoryId, userId))
                return Forbid();

            var vm = new ItemEditVM
            {
                Id = item.Id,
                InventoryId = item.InventoryId,
                Name = item.Name,
                Description = item.Description,
                CustomId = item.CustomId,
                Text1 = item.Text1, Text2 = item.Text2, Text3 = item.Text3,
                Number1 = item.Number1, Number2 = item.Number2, Number3 = item.Number3,
                Bool1 = item.Bool1, Bool2 = item.Bool2, Bool3 = item.Bool3,
                Date1 = item.Date1, Date2 = item.Date2, Date3 = item.Date3,
                Version = item.Version
            };

            return View(vm);
        }

        // POST: Item/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(ItemEditVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == vm.Id);
            if (item == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (!_accessService.CanEditItems(item.InventoryId, userId))
                return Forbid();

            item.Name = vm.Name;
            item.Description = vm.Description;
            item.Text1 = vm.Text1; item.Text2 = vm.Text2; item.Text3 = vm.Text3;
            item.Number1 = vm.Number1; item.Number2 = vm.Number2; item.Number3 = vm.Number3;
            item.Bool1 = vm.Bool1; item.Bool2 = vm.Bool2; item.Bool3 = vm.Bool3;
            item.Date1 = vm.Date1; item.Date2 = vm.Date2; item.Date3 = vm.Date3;

            try
            {
                // Set original RowVersion for optimistic concurrency detection
                _context.Entry(item).Property(i => i.Version).OriginalValue = vm.Version;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "This item was modified by someone else. Please reload and try again.");
                return View(vm);
            }

            return RedirectToAction("Details", "Inventory", new { id = item.InventoryId });
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

        // -----------------------------------------------------------------------
        // POST: Item/DeleteMultiple
        // Bulk-deletes items whose IDs are posted from the checkbox toolbar in
        // _ItemsTab.cshtml.  The form sends one hidden input named "ids" per
        // selected row, which ASP.NET Core model-binds as List<Guid>.
        //
        // WHY VERIFY ACCESS PER INVENTORY?
        // A crafted POST could mix IDs from inventories the user cannot edit.
        // We verify access for every distinct inventoryId in the batch so there
        // is no way to bypass the permission check by bundling foreign IDs.
        // -----------------------------------------------------------------------
        [HttpPost]
        public IActionResult DeleteMultiple(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return RedirectToAction("Index", "Inventory");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Load all matching items in one query
            var items = _context.Items
                .Where(i => ids.Contains(i.Id))
                .ToList();

            if (items.Count == 0)
                return RedirectToAction("Index", "Inventory");

            // Verify write access for every distinct parent inventory in the selection
            var distinctInventoryIds = items.Select(i => i.InventoryId).Distinct();
            foreach (var invId in distinctInventoryIds)
            {
                if (!_accessService.CanEditItems(invId, userId))
                    return Forbid();
            }

            // All access checks passed — delete the items
            _context.Items.RemoveRange(items);
            _context.SaveChanges();

            // Redirect to the first item's inventory (all items share one inventory
            // in the normal UI flow; the loop above handles the edge case anyway)
            var redirectInventoryId = items.First().InventoryId;
            return RedirectToAction("Details", "Inventory", new { id = redirectInventoryId });
        }

        // POST: Item/ToggleLike
        // Toggles a like for an item for the current logged-in user.
        // Returns the updated like count as JSON.
        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] LikeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var itemExists = await _context.Items.AnyAsync(i => i.Id == request.ItemId);
            if (!itemExists) return NotFound();

            var existingLike = await _context.ItemLikes
                .FirstOrDefaultAsync(l => l.ItemId == request.ItemId && l.UserId == userId);

            if (existingLike == null)
            {
                _context.ItemLikes.Add(new ItemLike
                {
                    Id = Guid.NewGuid(),
                    ItemId = request.ItemId,
                    UserId = userId
                });
            }
            else
            {
                _context.ItemLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.ItemLikes.CountAsync(l => l.ItemId == request.ItemId);
            return Json(new { likes = likeCount });
        }
    }

    public class LikeRequest
    {
        public Guid ItemId { get; set; }
    }
}
