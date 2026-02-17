
using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models.ViewModels
{
    // ViewModel for creating a new Item.
    // We use a ViewModel to ensure we only receive the necessary data from the form.
    // It also allows us to pass the InventoryId (hidden field in the form) seamlessly.
    public class ItemCreateViewModel
    {
        public Guid InventoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
