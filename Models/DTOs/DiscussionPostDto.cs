namespace InventoryManager.Models.DTOs
{
    // WHY A DTO?
    // The domain entity (DiscussionPost) is an EF model — it may have navigation
    // properties and fields the client doesn't need (e.g. UserId, InventoryId).
    // Sending the raw entity over SignalR risks:
    //   1. Exposing internal IDs unnecessarily.
    //   2. Circular reference issues if navigation properties are added later.
    // The DTO is a clean, minimal contract between server and browser.
    public class DiscussionPostDto
    {
        public Guid Id { get; set; }

        // Display name — safe to expose to the client
        public string UserName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        // ISO-8601 string is serialised automatically by System.Text.Json
        public DateTime CreatedAt { get; set; }
    }
}
