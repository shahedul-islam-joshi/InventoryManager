using InventoryManager.Data;
using InventoryManager.Helpers;
using InventoryManager.Models.Domain;
using InventoryManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services
{
    // AccessService is the single place where all write-access decisions are made.
    //
    // WHY KEEP LOGIC HERE AND NOT IN CONTROLLERS?
    // Controllers are responsible for HTTP concerns (routing, model binding, redirects).
    // Business rules like "who can edit this inventory" are not HTTP concerns.
    // Centralising them here means:
    //   1. The same rule applies consistently across all controllers.
    //   2. Rules can be changed in one place without touching controllers.
    //   3. The service can be unit-tested independently of HTTP infrastructure.
    public class AccessService : IAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccessService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -----------------------------------------------------------------------
        // CanEditInventory
        // Determines whether a user may edit inventory-level settings.
        //
        // Check order:
        //   1. Is owner              → always allowed
        //   2. Is admin              → always allowed
        //   3. Inventory is public AND user is authenticated → allowed
        //   4. Has explicit access grant in InventoryAccess table → allowed
        //   Otherwise               → denied
        // -----------------------------------------------------------------------
        public bool CanEditInventory(Guid inventoryId, string userId)
        {
            // Fetch the inventory to check ownership and visibility
            var inventory = _context.Inventories.FirstOrDefault(i => i.Id == inventoryId);
            if (inventory == null) return false;

            // 1. OWNER IS ALWAYS ALLOWED:
            // The owner created the inventory and is its ultimate authority.
            // This check must come first so the owner is never accidentally locked out.
            if (PermissionHelper.IsOwner(inventory, userId))
                return true;

            // 2. ADMINS ACT AS OWNERS:
            // High-privilege users must have full access to every inventory.
            var user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
            if (user != null && _userManager.IsInRoleAsync(user, "Admin").GetAwaiter().GetResult())
                return true;

            // 3. PUBLIC INVENTORY + AUTHENTICATED USER:
            // Any logged-in user may write to a public inventory.
            // We verify authentication by ensuring userId is non-null/non-empty,
            // which is only true when the request carries a valid identity cookie.
            if (inventory.IsPublic && !string.IsNullOrEmpty(userId))
                return true;

            // 4. EXPLICIT ACCESS GRANT:
            // Check the InventoryAccess table for a per-user grant.
            return _context.InventoryAccesses
                .Any(a => a.InventoryId == inventoryId && a.UserId == userId);
        }

        // -----------------------------------------------------------------------
        // CanEditItems
        // Determines whether a user may create, edit, or delete items.
        // Uses the same access rules as CanEditInventory.
        // Kept as a separate method so item-level and inventory-level rules
        // can diverge independently in the future without breaking callers.
        // -----------------------------------------------------------------------
        public bool CanEditItems(Guid inventoryId, string userId)
        {
            // Delegate to CanEditInventory — same rules apply for now
            return CanEditInventory(inventoryId, userId);
        }

        // -----------------------------------------------------------------------
        // GrantAccessAsync
        // Grants write access to the user identified by email.
        // Only the owner should call this — the controller enforces that.
        // -----------------------------------------------------------------------
        public async Task GrantAccessAsync(Guid inventoryId, string userEmail)
        {
            // Look up the user by email using Identity's UserManager
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
                throw new InvalidOperationException($"No user found with email '{userEmail}'.");

            // Prevent duplicate grants — silently skip if already granted
            bool alreadyGranted = await _context.InventoryAccesses
                .AnyAsync(a => a.InventoryId == inventoryId && a.UserId == user.Id);

            if (alreadyGranted)
                throw new InvalidOperationException($"User '{userEmail}' already has access.");

            var access = new InventoryAccess
            {
                Id = Guid.NewGuid(),
                InventoryId = inventoryId,
                UserId = user.Id,
                GrantedAt = DateTime.UtcNow
            };

            _context.InventoryAccesses.Add(access);
            await _context.SaveChangesAsync();
        }

        // -----------------------------------------------------------------------
        // RemoveAccessAsync
        // Revokes write access for the given userId.
        // Only the owner should call this — the controller enforces that.
        // -----------------------------------------------------------------------
        public async Task RemoveAccessAsync(Guid inventoryId, string userId)
        {
            var access = await _context.InventoryAccesses
                .FirstOrDefaultAsync(a => a.InventoryId == inventoryId && a.UserId == userId);

            if (access != null)
            {
                _context.InventoryAccesses.Remove(access);
                await _context.SaveChangesAsync();
            }
            // If no record found, nothing to remove — treat as a no-op
        }

        // -----------------------------------------------------------------------
        // GetUsersWithAccessAsync
        // Returns the ApplicationUser objects for all explicitly granted users.
        // The owner is NOT included — they have permanent access by definition.
        // -----------------------------------------------------------------------
        public async Task<List<ApplicationUser>> GetUsersWithAccessAsync(Guid inventoryId)
        {
            // Get the user IDs from the access table
            var userIds = await _context.InventoryAccesses
                .Where(a => a.InventoryId == inventoryId)
                .Select(a => a.UserId)
                .ToListAsync();

            if (!userIds.Any())
                return new List<ApplicationUser>();

            // Fetch the full user objects from Identity
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Cast<ApplicationUser>()
                .ToListAsync();

            return users;
        }
    }
}
