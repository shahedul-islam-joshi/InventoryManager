using InventoryManager.Models.DTOs;

namespace InventoryManager.Models.ViewModels
{
    // ViewModel passed to the _DiscussionTab partial view.
    // Carries the inventory ID (for JS group joining) and the pre-loaded posts
    // (rendered server-side so guests can read without JavaScript).
    public class DiscussionTabViewModel
    {
        public Guid InventoryId { get; set; }

        // Posts loaded from the database on page load — oldest first
        public List<DiscussionPostDto> Posts { get; set; } = new List<DiscussionPostDto>();
    }
}
