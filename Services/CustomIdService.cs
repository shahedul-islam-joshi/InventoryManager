using InventoryManager.Data;
using InventoryManager.Helpers;
using InventoryManager.Models.Domain;
using InventoryManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventoryManager.Services
{
    // Generates custom IDs from an inventory's IdElement template chain.
    // Each IdElement represents one segment (Fixed text, Random string, Sequence number, Date).
    // Segments are concatenated in order to form the final ID.
    public class CustomIdService : ICustomIdService
    {
        private readonly ApplicationDbContext _context;

        public CustomIdService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GenerateAsync(Guid inventoryId)
        {
            var elements = await _context.IdElements
                .Where(e => e.InventoryId == inventoryId)
                .OrderBy(e => e.Order)
                .ToListAsync();

            if (!elements.Any())
                return null;

            return await BuildIdAsync(inventoryId, elements);
        }

        public async Task<bool> IsUniqueAsync(Guid inventoryId, string customId, Guid? excludeItemId = null)
        {
            var query = _context.Items.Where(i => i.InventoryId == inventoryId && i.CustomId == customId);
            if (excludeItemId.HasValue)
                query = query.Where(i => i.Id != excludeItemId.Value);
            return !await query.AnyAsync();
        }

        public async Task<string> PreviewAsync(Guid inventoryId)
        {
            var elements = await _context.IdElements
                .Where(e => e.InventoryId == inventoryId)
                .OrderBy(e => e.Order)
                .ToListAsync();

            if (!elements.Any())
                return "(no template defined)";

            return await BuildIdAsync(inventoryId, elements, isPreview: true);
        }

        // -----------------------------------------------------------------------
        // Private: builds the ID string from the element list
        // -----------------------------------------------------------------------
        private async Task<string> BuildIdAsync(Guid inventoryId, List<IdElement> elements, bool isPreview = false)
        {
            var parts = new List<string>();

            foreach (var element in elements)
            {
                parts.Add(element.Type switch
                {
                    "Fixed" => element.Format ?? string.Empty,
                    "Random" => IdGeneratorHelper.RandomAlphanumeric(ParseLength(element.Format, 6)),
                    "Date" => DateTime.UtcNow.ToString(element.Format ?? "yyyyMMdd"),
                    "Sequence" => await NextSequenceAsync(inventoryId, element.Format, isPreview),
                    _ => string.Empty
                });
            }

            return string.Concat(parts);
        }

        private async Task<string> NextSequenceAsync(Guid inventoryId, string? format, bool isPreview)
        {
            var seq = await _context.InventorySequences.FirstOrDefaultAsync(s => s.InventoryId == inventoryId);
            if (seq == null)
            {
                seq = new InventorySequence { InventoryId = inventoryId, CurrentValue = 0 };
                _context.InventorySequences.Add(seq);
            }

            if (!isPreview)
                seq.CurrentValue++;

            await _context.SaveChangesAsync();

            var padWidth = ParseLength(format, 4);
            return (isPreview ? seq.CurrentValue + 1 : seq.CurrentValue).ToString().PadLeft(padWidth, '0');
        }

        private static int ParseLength(string? format, int fallback)
        {
            if (int.TryParse(format, out var length) && length > 0)
                return length;
            return fallback;
        }
    }
}
