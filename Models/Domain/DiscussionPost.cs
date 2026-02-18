namespace InventoryManager.Models.Domain
{
    // Represents a single message posted in an inventory's discussion.
    // UserName is stored directly (denormalised) so we don't need a JOIN to
    // AspNetUsers every time we load the discussion feed.
    public class DiscussionPost
    {
        public Guid Id { get; set; }

        // Which inventory this message belongs to
        public Guid InventoryId { get; set; }

        // Identity user ID of the author
        public string UserId { get; set; } = string.Empty;

        // Display name stored at write-time — avoids a JOIN on every read
        public string UserName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
