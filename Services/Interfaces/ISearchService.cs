using InventoryManager.Models.ViewModels;

namespace InventoryManager.Services.Interfaces
{
    // ISearchService defines the contract for full-text search.
    //
    // WHY A SERVICE INTERFACE?
    // The controller depends on this abstraction, not the concrete implementation.
    // This keeps the controller thin and makes the search logic independently testable.
    // If we ever switch from raw SQL to a dedicated search engine (e.g. Elasticsearch),
    // only SearchService changes — the controller and view are untouched.
    public interface ISearchService
    {
        // Executes a full-text search across Inventories and Items.
        // Returns a paginated, ranked SearchPageVM ready for the view.
        //
        // WHY SERVICE LAYER HANDLES SEARCH?
        // Full-text search involves raw SQL, pagination math, and DTO mapping.
        // None of these are HTTP concerns — they belong in the service layer,
        // not in a controller action.
        Task<SearchPageVM> SearchAsync(string query, int page, int pageSize);
    }
}
