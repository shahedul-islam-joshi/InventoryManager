using InventoryManager.Data;
using InventoryManager.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Views.Shared.Components
{
    // ViewComponent that renders a table of inventories.
    // Usage: @await Component.InvokeAsync("InventoryTable", new { inventories = myList })
    public class InventoryTableViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Inventory> inventories)
        {
            return View(inventories);
        }
    }
}
