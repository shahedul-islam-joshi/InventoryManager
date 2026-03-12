using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;

namespace InventoryManager.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetLatestPublicAsync(int count);
        Task<List<Inventory>> GetByOwnerAsync(string ownerId);
        Task<Inventory?> GetByIdAsync(Guid id);
        Task CreateAsync(InventoryCreateVM vm, string ownerId);
        Task UpdateAsync(InventoryEditVM vm);
        Task DeleteAsync(Guid id, string requestingUserId);
    }
}
