using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.ViewModels
{
    public class InventoryCreateVM
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsPublic { get; set; }

        // Tags as a comma-separated string from the UI
        public string? Tags { get; set; }
    }
}
