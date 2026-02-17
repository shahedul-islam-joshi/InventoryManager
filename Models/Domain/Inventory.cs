using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.Domain
{
    public class Inventory
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string OwnerId { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsPublic { get; set; }
    }
}
