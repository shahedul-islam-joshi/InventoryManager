using InventoryManager.Data;
using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManager.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Item/Create?inventoryId=...
        // Displays the form to create a new Item for a specific Inventory
        [HttpGet]
        public IActionResult Create(Guid inventoryId)
        {
            // We initialize the ViewModel with the InventoryId so it can be passed to the view (usually as a hidden field)
            var model = new ItemCreateViewModel
            {
                InventoryId = inventoryId
            };
            return View(model);
        }

        // POST: Item/Create
        // Handles the submission of the Create Item form
        [HttpPost]
        public IActionResult Create(ItemCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verify that the current user is the owner of the inventory
                // We must query the database to find the inventory and check its OwnerId
                var inventory = _context.Inventories.FirstOrDefault(i => i.Id == model.InventoryId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (inventory != null && inventory.OwnerId == userId)
                {
                    // Create the new Item entity from the ViewModel
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

                    // Redirect back to the Inventory Details page
                    return RedirectToAction("Details", "Inventory", new { id = model.InventoryId });
                }
                else
                {
                    // If inventory not found or user is not owner, return Unauthorized or NotFound
                    return Unauthorized();
                }
            }

            // If validation fails, return the view with the model to show errors
            return View(model);
        }

        // POST: Item/Delete/5
        // Deletes an item
        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            // Find the item
            var item = _context.Items.FirstOrDefault(i => i.Id == id);

            if (item != null)
            {
                // Find the associated inventory to check ownership
                var inventory = _context.Inventories.FirstOrDefault(i => i.Id == item.InventoryId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (inventory != null && inventory.OwnerId == userId)
                {
                    _context.Items.Remove(item);
                    _context.SaveChanges();

                    // Redirect back to the Inventory Details page
                    return RedirectToAction("Details", "Inventory", new { id = item.InventoryId });
                }
                else
                {
                    return Unauthorized();
                }
            }

            // If item not found, just redirect back (or show error)
            // Redirecting to Inventory Index as a fallback if we can't find the item/inventory to redirect to details
            return RedirectToAction("Index", "Inventory");
        }
    }
}
