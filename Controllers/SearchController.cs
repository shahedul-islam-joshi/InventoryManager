using InventoryManager.Models.ViewModels;
using InventoryManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers
{
    // SearchController handles the search results page.
    //
    // WHY CONTROLLER STAYS THIN:
    // All SQL, pagination math, and result mapping live in ISearchService.
    // The controller's only jobs are:
    //   1. Validate the query string (reject empty queries early)
    //   2. Call the service
    //   3. Return the view
    // This follows the same pattern as the rest of the application.
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // GET: /Search/Results?q=...&page=1
        // Accepts a plain-text query and an optional page number.
        // No [Authorize] — search is available to all users including guests.
        [HttpGet]
        public async Task<IActionResult> Results(string q, int page = 1)
        {
            // Return an empty view model for blank queries — no SQL executed
            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new SearchPageVM { Query = q ?? string.Empty });
            }

            // Delegate all search logic to the service — controller does no SQL
            var result = await _searchService.SearchAsync(q.Trim(), page, pageSize: 10);
            return View(result);
        }
    }
}
