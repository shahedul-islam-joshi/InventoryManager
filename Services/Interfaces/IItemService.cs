using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;

namespace InventoryManager.Services.Interfaces
{
    public interface IItemService
    {
        Task<Item?> GetByIdAsync(Guid id);
        Task CreateAsync(ItemCreateViewModel vm, string userId);
        Task UpdateAsync(ItemEditVM vm, string userId);
        Task DeleteAsync(Guid id, string userId);
    }
}
