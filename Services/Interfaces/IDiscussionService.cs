using InventoryManager.Models.DTOs;

namespace InventoryManager.Services.Interfaces
{
    // IDiscussionService defines the contract for saving and retrieving discussion posts.
    //
    // WHY AN INTERFACE?
    // DiscussionHub depends on this abstraction rather than the concrete service.
    // This keeps the Hub thin and makes the service independently testable.
    public interface IDiscussionService
    {
        // Returns all posts for an inventory, ordered oldest-first (chat order).
        Task<List<DiscussionPostDto>> GetPostsAsync(Guid inventoryId);

        // Saves a new post and returns the DTO ready to broadcast via SignalR.
        Task<DiscussionPostDto> AddPostAsync(Guid inventoryId, string userId, string content);
    }
}
