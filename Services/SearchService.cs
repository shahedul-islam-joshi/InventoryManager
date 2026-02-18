using InventoryManager.Data;
using InventoryManager.Models.ViewModels;
using InventoryManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services
{
    // SearchService executes full-text search using PostgreSQL's native FTS engine.
    //
    // WHY RAW SQL (FromSqlRaw) AND NOT EF CORE LINQ?
    // EF Core's LINQ provider has no translation for PostgreSQL FTS functions:
    //   - to_tsvector()    — builds a weighted document vector from text columns
    //   - plainto_tsquery() — converts plain user input to a FTS query safely
    //   - ts_rank()        — computes a relevance score for result ordering
    // These functions are PostgreSQL-specific and cannot be expressed as LINQ.
    // FromSqlRaw lets us write the exact SQL we need while still using EF's
    // connection management, parameter binding, and result materialisation.
    //
    // WHY plainto_tsquery AND NOT to_tsquery?
    // to_tsquery requires FTS operator syntax (e.g. "red & shoes").
    // Plain user input like "red shoes" would throw a syntax error.
    // plainto_tsquery treats the input as a phrase and handles it safely.
    //
    // WHY SERVICE LAYER HANDLES SEARCH?
    // Raw SQL, pagination math, and DTO mapping are business/data concerns,
    // not HTTP concerns. Keeping them here means the controller stays thin
    // and the search logic can be tested independently.
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SearchPageVM> SearchAsync(string query, int page, int pageSize)
        {
            // Clamp page to a valid range
            if (page < 1) page = 1;

            // -----------------------------------------------------------------------
            // INVENTORY SEARCH
            // Searches Title and Description columns.
            // Concatenates them with a space so both contribute to the tsvector.
            // Uses {0} parameter placeholder — EF Core passes it as a safe SQL parameter,
            // preventing SQL injection even with raw SQL.
            // -----------------------------------------------------------------------
            var inventoryResults = await _context.Database
                .SqlQueryRaw<SearchRawRow>(
                    @"SELECT
                        ""Id""::text           AS ""Id"",
                        NULL::text             AS ""InventoryId"",
                        'Inventory'            AS ""Type"",
                        ""Title""              AS ""Title"",
                        LEFT(""Description"", 200) AS ""Snippet"",
                        ts_rank(
                            to_tsvector('english', COALESCE(""Title"", '') || ' ' || COALESCE(""Description"", '')),
                            plainto_tsquery('english', {0})
                        )                      AS ""Rank""
                    FROM ""Inventories""
                    WHERE to_tsvector('english', COALESCE(""Title"", '') || ' ' || COALESCE(""Description"", ''))
                          @@ plainto_tsquery('english', {0})",
                    query)
                .ToListAsync();

            // -----------------------------------------------------------------------
            // ITEM SEARCH
            // Searches Name and Description columns.
            // InventoryId is included so the view can build a link to the parent inventory.
            // -----------------------------------------------------------------------
            var itemResults = await _context.Database
                .SqlQueryRaw<SearchRawRow>(
                    @"SELECT
                        ""Id""::text              AS ""Id"",
                        ""InventoryId""::text     AS ""InventoryId"",
                        'Item'                    AS ""Type"",
                        ""Name""                  AS ""Title"",
                        LEFT(""Description"", 200) AS ""Snippet"",
                        ts_rank(
                            to_tsvector('english', COALESCE(""Name"", '') || ' ' || COALESCE(""Description"", '')),
                            plainto_tsquery('english', {0})
                        )                         AS ""Rank""
                    FROM ""Items""
                    WHERE to_tsvector('english', COALESCE(""Name"", '') || ' ' || COALESCE(""Description"", ''))
                          @@ plainto_tsquery('english', {0})",
                    query)
                .ToListAsync();

            // -----------------------------------------------------------------------
            // COMBINE, RANK, PAGINATE
            // Both result sets are merged in memory, sorted by rank descending,
            // then sliced to the requested page.
            //
            // WHY MERGE IN MEMORY?
            // A UNION ALL in raw SQL across two different tables is possible but
            // makes the SQL harder to read and maintain. Since result sets are
            // typically small (FTS filters aggressively), in-memory merge is fine.
            // -----------------------------------------------------------------------
            var allResults = inventoryResults
                .Concat(itemResults)
                .OrderByDescending(r => r.Rank)
                .ToList();

            int totalCount = allResults.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            // Clamp page to valid range after we know totalPages
            if (page > totalPages) page = totalPages;

            var pageResults = allResults
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new SearchResultVM
                {
                    Type = r.Type,
                    Id = Guid.Parse(r.Id),
                    InventoryId = r.InventoryId != null ? Guid.Parse(r.InventoryId) : null,
                    Title = r.Title,
                    Snippet = r.Snippet,
                    Rank = (double)r.Rank
                })
                .ToList();

            return new SearchPageVM
            {
                Query = query,
                Results = pageResults,
                Page = page,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        // -----------------------------------------------------------------------
        // SearchRawRow — internal projection class for SqlQueryRaw
        // EF Core's SqlQueryRaw requires a class with properties matching the
        // column names returned by the SQL query. This class is private to the
        // service — it is never exposed outside.
        // -----------------------------------------------------------------------
        private class SearchRawRow
        {
            public string Id { get; set; } = string.Empty;
            public string? InventoryId { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Snippet { get; set; } = string.Empty;
            public float Rank { get; set; }
        }
    }
}
