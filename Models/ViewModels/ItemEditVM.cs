using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.ViewModels
{
    public class ItemEditVM
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        // Custom ID (read-only in edit — generated once on create)
        public string? CustomId { get; set; }

        // Text slots
        public string? Text1 { get; set; }
        public string? Text2 { get; set; }
        public string? Text3 { get; set; }

        // Number slots
        public decimal? Number1 { get; set; }
        public decimal? Number2 { get; set; }
        public decimal? Number3 { get; set; }

        // Boolean slots
        public bool? Bool1 { get; set; }
        public bool? Bool2 { get; set; }
        public bool? Bool3 { get; set; }

        // Date slots
        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? Date3 { get; set; }

        // For optimistic concurrency
        public byte[]? Version { get; set; }
    }
}
