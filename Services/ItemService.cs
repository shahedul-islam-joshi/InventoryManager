using InventoryManager.Data;
using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;
using InventoryManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services
{
    public class ItemService : IItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccessService _accessService;
        private readonly ICustomIdService _customIdService;

        public ItemService(ApplicationDbContext context, IAccessService accessService, ICustomIdService customIdService)
        {
            _context = context;
            _accessService = accessService;
            _customIdService = customIdService;
        }

        public async Task<Item?> GetByIdAsync(Guid id)
        {
            return await _context.Items
                .Include(i => i.ItemLikes)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreateAsync(ItemCreateViewModel vm, string userId)
        {
            if (!_accessService.CanEditItems(vm.InventoryId, userId))
                throw new UnauthorizedAccessException("No write access to this inventory.");

            var customId = await _customIdService.GenerateAsync(vm.InventoryId);

            var item = new Item
            {
                Id = Guid.NewGuid(),
                InventoryId = vm.InventoryId,
                Name = vm.Name,
                Description = vm.Description,
                CustomId = customId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemEditVM vm, string userId)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == vm.Id)
                ?? throw new InvalidOperationException("Item not found.");

            if (!_accessService.CanEditItems(item.InventoryId, userId))
                throw new UnauthorizedAccessException("No write access to this inventory.");

            item.Name = vm.Name;
            item.Description = vm.Description;
            item.Text1 = vm.Text1; item.Text2 = vm.Text2; item.Text3 = vm.Text3;
            item.Number1 = vm.Number1; item.Number2 = vm.Number2; item.Number3 = vm.Number3;
            item.Bool1 = vm.Bool1; item.Bool2 = vm.Bool2; item.Bool3 = vm.Bool3;
            item.Date1 = vm.Date1; item.Date2 = vm.Date2; item.Date3 = vm.Date3;

            // Set the concurrency token from the form so EF can detect conflicts
            _context.Entry(item).Property(i => i.Version).OriginalValue = vm.Version;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new InvalidOperationException("Item not found.");

            if (!_accessService.CanEditItems(item.InventoryId, userId))
                throw new UnauthorizedAccessException("No write access to this inventory.");

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
