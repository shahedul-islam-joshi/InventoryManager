using InventoryManager.Models.Domain;
using InventoryManager.Models.DTOs;

namespace InventoryManager.Services.Interfaces
{
    public interface ITagService
    {
        /// <summary>Returns tags whose names start with the given prefix (for autocomplete).</summary>
        Task<List<TagAutocompleteDto>> SearchAsync(string prefix, int limit = 10);

        /// <summary>
        /// Syncs the tag list for an inventory.
        /// Creates new tags as needed, removes old ones, and adds new associations.
        /// </summary>
        Task SyncTagsAsync(Guid inventoryId, IEnumerable<string> tagNames);

        /// <summary>Returns all tags with their usage counts (for the tag cloud).</summary>
        Task<List<(Tag Tag, int Count)>> GetTagCloudAsync();
    }
}
