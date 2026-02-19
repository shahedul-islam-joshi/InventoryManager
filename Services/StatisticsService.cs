using InventoryManager.Data;
using InventoryManager.Models.Domain;
using InventoryManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public int GetTotalItems(Guid inventoryId)
        {
            return _context.Items.Count(i => i.InventoryId == inventoryId);
        }

        public int GetTotalLikes(Guid inventoryId)
        {
            return _context.ItemLikes
                .Count(l => _context.Items.Any(i => i.Id == l.ItemId && i.InventoryId == inventoryId));
        }

        public Item? GetMostLikedItem(Guid inventoryId)
        {
            return _context.Items
                .Include(i => i.ItemLikes)
                .Where(i => i.InventoryId == inventoryId)
                .OrderByDescending(i => i.ItemLikes.Count)
                .FirstOrDefault();
        }

        public Item? GetLatestItem(Guid inventoryId)
        {
            return _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefault();
        }
    }
}
