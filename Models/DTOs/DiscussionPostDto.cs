namespace InventoryManager.Models.DTOs
{
    // WHY A DTO?
    // The domain entity (DiscussionPost) is an EF model that may have navigation
    // properties we don't want to send over the wire (e.g. InventoryId, the full
    // user graph). The DTO is a clean, minimal contract between server and browser.
    public class DiscussionPostDto
    {
        public Guid Id { get; set; }

        // UserId is included so the client can build a link to the author's profile page.
        public string UserId { get; set; } = string.Empty;

        // Display name — rendered as the link text in the discussion UI
        public string UserName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        // ISO-8601 string is serialised automatically by System.Text.Json
        public DateTime CreatedAt { get; set; }
    }
}
