using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.ViewModels
{
    public class InventoryEditVM
    {
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsPublic { get; set; }

        // Comma-separated tag names for the tag input widget
        public string? Tags { get; set; }

        // RowVersion for optimistic concurrency — hidden field in the form
        public byte[]? Version { get; set; }
    }
}
