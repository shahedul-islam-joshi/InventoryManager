using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManager.Data;
using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;
using InventoryManager.Services.Interfaces;
using InventoryManager.Helpers;
using System.Security.Claims;

namespace InventoryManager.Controllers
{
    // WHY CONTROLLERS STAY THIN:
    // Controllers handle HTTP concerns only: routing, model binding, redirects, and HTTP status codes.
    // All business/permission logic lives in AccessService.
    // This separation means permission rules can change without touching controller code.
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccessService _accessService;

        // IAccessService is injected — the controller never instantiates it directly.
        // This follows the Dependency Inversion principle and keeps the controller testable.
        public InventoryController(ApplicationDbContext context, IAccessService accessService)
        {
            _context = context;
            _accessService = accessService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var inventories = await _context.Inventories.ToListAsync();
            return View(inventories);
        }

        // GET: Inventory/Details/5
        // Builds the InventoryDetailsViewModel with ownership and access flags
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var inventory = await _context.Inventories.FirstOrDefaultAsync(m => m.Id == id);
            if (inventory == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // PermissionHelper.IsOwner keeps the ownership check in one place
            bool isOwner = PermissionHelper.IsOwner(inventory, userId);

            // CanEditItems delegates to AccessService — owner OR granted user returns true
            bool canEdit = _accessService.CanEditItems(inventory.Id, userId);

            var items = await _context.Items
                .Where(i => i.InventoryId == id)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            // Only load the access list when the current user is the owner.
            // Non-owners never see the Access tab, so there is no need to query it for them.
            var usersWithAccess = isOwner
                ? await _accessService.GetUsersWithAccessAsync(inventory.Id)
                : new List<ApplicationUser>();

            var viewModel = new InventoryDetailsViewModel
            {
                Inventory = inventory,
                IsOwner = isOwner,
                CanEdit = canEdit,
                Items = items,
                UsersWithAccess = usersWithAccess
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            inventory.Id = Guid.NewGuid();
            inventory.CreatedAt = DateTime.UtcNow;
            inventory.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ModelState.Remove("Id");
            ModelState.Remove("OwnerId");
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(x => x.Id == id);

            if (inventory != null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Only the owner can delete an inventory — access grants do not include deletion
                if (!PermissionHelper.IsOwner(inventory, userId))
                    return Forbid();

                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST: Inventory/GrantAccess
        // Grants write access to a user by email.
        //
        // WHY OWNER-ONLY CHECK IN CONTROLLER?
        // The controller is responsible for verifying the HTTP request is authorised
        // (i.e., the requester is the owner). The actual grant logic lives in AccessService.
        // -----------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> GrantAccess(Guid inventoryId, string email)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);
            if (inventory == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Only the owner can manage access — return 403 for anyone else
            if (!PermissionHelper.IsOwner(inventory, userId))
                return Forbid();

            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    await _accessService.GrantAccessAsync(inventoryId, email);
                    TempData["AccessMessage"] = $"Access granted to {email}.";
                }
                catch (InvalidOperationException ex)
                {
                    // Surface the service error (user not found, already granted) to the view
                    TempData["AccessError"] = ex.Message;
                }
            }

            // Redirect back to Details, opening the Access tab
            return RedirectToAction(nameof(Details), new { id = inventoryId });
        }

        // -----------------------------------------------------------------------
        // POST: Inventory/RemoveAccess
        // Revokes write access for a specific user.
        // Only the owner can call this action.
        // -----------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> RemoveAccess(Guid inventoryId, string targetUserId)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);
            if (inventory == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Only the owner can remove access
            if (!PermissionHelper.IsOwner(inventory, userId))
                return Forbid();

            await _accessService.RemoveAccessAsync(inventoryId, targetUserId);
            TempData["AccessMessage"] = "Access removed.";

            return RedirectToAction(nameof(Details), new { id = inventoryId });
        }
    }
}
