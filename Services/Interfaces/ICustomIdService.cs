using InventoryManager.Models.Domain;

namespace InventoryManager.Services.Interfaces
{
    public interface ICustomIdService
    {
        /// <summary>Generates the custom ID for a new item in this inventory.</summary>
        Task<string?> GenerateAsync(Guid inventoryId);

        /// <summary>Validates that a proposed custom ID is unique within the inventory.</summary>
        Task<bool> IsUniqueAsync(Guid inventoryId, string customId, Guid? excludeItemId = null);

        /// <summary>Returns a preview of how the current template would generate an ID.</summary>
        Task<string> PreviewAsync(Guid inventoryId);
    }
}
