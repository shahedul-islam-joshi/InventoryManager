using InventoryManager.Models.Domain;

namespace InventoryManager.Services.Interfaces
{
    public interface IStatisticsService
    {
        int GetTotalItems(Guid inventoryId);
        int GetTotalLikes(Guid inventoryId);
        Item? GetMostLikedItem(Guid inventoryId);
        Item? GetLatestItem(Guid inventoryId);
    }
}
