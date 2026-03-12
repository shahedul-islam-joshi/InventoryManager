using InventoryManager.Data;
using InventoryManager.Helpers;
using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;
using InventoryManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITagService _tagService;

        public InventoryService(ApplicationDbContext context, ITagService tagService)
        {
            _context = context;
            _tagService = tagService;
        }

        public async Task<List<Inventory>> GetLatestPublicAsync(int count)
        {
            return await _context.Inventories
                .Where(i => i.IsPublic)
                .OrderByDescending(i => i.CreatedAt)
                .Take(count)
                .Include(i => i.InventoryTags).ThenInclude(it => it.Tag)
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetByOwnerAsync(string ownerId)
        {
            return await _context.Inventories
                .Where(i => i.OwnerId == ownerId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByIdAsync(Guid id)
        {
            return await _context.Inventories
                .Include(i => i.InventoryTags).ThenInclude(it => it.Tag)
                .Include(i => i.Fields)
                .Include(i => i.IdElements)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreateAsync(InventoryCreateVM vm, string ownerId)
        {
            var inventory = new Inventory
            {
                Id = Guid.NewGuid(),
                Title = vm.Title,
                Description = vm.Description,
                Category = vm.Category,
                ImageUrl = vm.ImageUrl,
                IsPublic = vm.IsPublic,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Sync tags after saving so we have the inventory ID
            if (!string.IsNullOrWhiteSpace(vm.Tags))
            {
                var tagNames = vm.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                await _tagService.SyncTagsAsync(inventory.Id, tagNames);
            }
        }

        public async Task UpdateAsync(InventoryEditVM vm)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == vm.Id)
                ?? throw new InvalidOperationException("Inventory not found.");

            inventory.Title = vm.Title;
            inventory.Description = vm.Description;
            inventory.Category = vm.Category;
            inventory.ImageUrl = vm.ImageUrl;
            inventory.IsPublic = vm.IsPublic;

            await _context.SaveChangesAsync();

            // Sync tags
            var tagNames = string.IsNullOrWhiteSpace(vm.Tags)
                ? Enumerable.Empty<string>()
                : vm.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            await _tagService.SyncTagsAsync(inventory.Id, tagNames);
        }

        public async Task DeleteAsync(Guid id, string requestingUserId)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new InvalidOperationException("Inventory not found.");

            if (!PermissionHelper.IsOwner(inventory, requestingUserId))
                throw new UnauthorizedAccessException("Only the owner can delete an inventory.");

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }
    }
}
