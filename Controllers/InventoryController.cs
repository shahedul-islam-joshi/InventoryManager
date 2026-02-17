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
            // Set required fields that are not in form
            inventory.Id = Guid.NewGuid();
            inventory.CreatedAt = DateTime.Now;
            inventory.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Removing Validations for OwnerId and Id since we set them here
            ModelState.Remove("OwnerId");
            // Id is Guid, defaults to all 0s if not set, but we set it.
            // However, ModelState validation happens before we set them?
            // Actually, if we bind the model, the binder validates.
            // OwnerId is string, might be required if nullable not set.
            // In Entity, string is reference type, so it is required by default in recent .NET unless marked nullable?
            // Let's make sure we handle this.
            
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
