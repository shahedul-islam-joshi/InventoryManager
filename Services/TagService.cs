using InventoryManager.Data;
using InventoryManager.Models.Domain;
using InventoryManager.Models.DTOs;
using InventoryManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;

        public TagService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TagAutocompleteDto>> SearchAsync(string prefix, int limit = 10)
        {
            var lower = prefix.ToLowerInvariant();
            return await _context.Tags
                .Where(t => t.Name.StartsWith(lower))
                .OrderBy(t => t.Name)
                .Take(limit)
                .Select(t => new TagAutocompleteDto { Id = t.Id, Text = t.Name })
                .ToListAsync();
        }

        public async Task SyncTagsAsync(Guid inventoryId, IEnumerable<string> tagNames)
        {
            // Normalise to lowercase and deduplicate
            var desiredNames = tagNames
                .Select(n => n.Trim().ToLowerInvariant())
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();

            // Remove existing associations
            var existing = await _context.InventoryTags
                .Where(it => it.InventoryId == inventoryId)
                .ToListAsync();
            _context.InventoryTags.RemoveRange(existing);

            // Ensure all desired tags exist
            foreach (var name in desiredNames)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
                if (tag == null)
                {
                    tag = new Tag { Name = name };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                _context.InventoryTags.Add(new InventoryTag
                {
                    InventoryId = inventoryId,
                    TagId = tag.Id
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<(Tag Tag, int Count)>> GetTagCloudAsync()
        {
            var rawList = await _context.Tags
                .Select(t => new { Tag = t, Count = t.InventoryTags.Count })
                .Where(x => x.Count > 0)
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return rawList.Select(x => (x.Tag, x.Count)).ToList();
        }
    }
}
