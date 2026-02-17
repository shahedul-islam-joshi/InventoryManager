using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManager.Data;
using InventoryManager.Models.Domain;
using System.Security.Claims;

namespace InventoryManager.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var inventories = await _context.Inventories.ToListAsync();
            return View(inventories);
        }

        // GET: Inventory/Details/5
        // Shows the details of an inventory item including items, discussion, and settings
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get inventory from database
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inventory == null)
            {
                return NotFound();
            }

            // Get current logged-in user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Determine if user is owner
            // We check this in the controller to securely pass this flag to the view
            // The view will use this to show/hide the Settings tab
            bool isOwner = (inventory.OwnerId == userId);

            // Fetch items related to this inventory
            var items = await _context.Items
                .Where(i => i.InventoryId == id)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            // Create InventoryDetailsViewModel
            // Using a ViewModel allows us to pass 'IsOwner' alongside the 'Inventory' object
            var viewModel = new InventoryManager.Models.ViewModels.InventoryDetailsViewModel
            {
                Inventory = inventory,
                IsOwner = isOwner,
                Items = items
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
            // 1. Manually set the values
            inventory.Id = Guid.NewGuid();
            inventory.CreatedAt = DateTime.UtcNow; // Use UtcNow for databases like PostgreSQL
            inventory.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Clear validation for fields the user didn't type in
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
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (inventory.OwnerId == userId)
                {
                    _context.Inventories.Remove(inventory);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return Unauthorized();
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
