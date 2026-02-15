using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
