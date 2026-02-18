namespace InventoryManager.Models.ViewModels
{
    // Represents a single search result — either an Inventory or an Item.
    // WHY A VIEWMODEL AND NOT A DOMAIN ENTITY?
    // Domain entities (Inventory, Item) have different shapes and different fields.
    // A unified ViewModel lets the view iterate a single list without branching on type.
    // It also decouples the view from EF model changes.
    public class SearchResultVM
    {
        // "Inventory" or "Item" — used to render the type badge and build the correct link
        public string Type { get; set; } = string.Empty;

        // Primary key of the result (Inventory.Id or Item.Id)
        public Guid Id { get; set; }

        // For Item results: the parent inventory ID needed to build the Details link
        // Null for Inventory results
        public Guid? InventoryId { get; set; }

        // The main heading shown in the result card (Inventory.Title or Item.Name)
        public string Title { get; set; } = string.Empty;

        // A short excerpt of the matching text shown below the title
        public string Snippet { get; set; } = string.Empty;

        // ts_rank score from PostgreSQL — higher means more relevant
        // WHY RANK?
        // Without ranking, results are returned in arbitrary storage order.
        // ts_rank scores each result by term frequency and position, so the most
        // relevant results appear first — critical for a good search experience.
        public double Rank { get; set; }
    }

    // Wraps the result list with pagination metadata for the view.
    // Keeping pagination data here avoids ViewBag/ViewData and keeps the view strongly typed.
    public class SearchPageVM
    {
        // The original query string — pre-filled in the search box
        public string Query { get; set; } = string.Empty;

        // The results for the current page, sorted by Rank DESC
        public List<SearchResultVM> Results { get; set; } = new();

        // Current page number (1-based)
        public int Page { get; set; } = 1;

        // Total number of pages
        public int TotalPages { get; set; }

        // Total number of matching results across all pages
        public int TotalCount { get; set; }
    }
}
