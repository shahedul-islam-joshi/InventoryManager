using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.Domain
{
    public class Inventory
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // Add '?' to make these optional for the Form Validator
        public string? OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
    }
}
