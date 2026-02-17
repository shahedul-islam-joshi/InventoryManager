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
