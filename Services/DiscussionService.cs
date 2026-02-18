using InventoryManager.Data;
using InventoryManager.Models.Domain;
using InventoryManager.Models.DTOs;
using InventoryManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Services
{
    // DiscussionService is the single place where discussion messages are persisted.
    //
    // WHY KEEP DB ACCESS HERE AND NOT IN THE HUB?
    // SignalR Hubs are transient — a new Hub instance is created for every connection.
    // ApplicationDbContext is scoped (one per HTTP request).
    // Injecting a scoped service into a transient Hub causes a lifetime mismatch
    // and can lead to concurrency bugs. The service is injected as scoped and
    // resolved correctly by the DI container when called from the Hub.
    public class DiscussionService : IDiscussionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -----------------------------------------------------------------------
        // GetPostsAsync
        // Returns all posts for a given inventory, oldest-first (natural chat order).
        // -----------------------------------------------------------------------
        public async Task<List<DiscussionPostDto>> GetPostsAsync(Guid inventoryId)
        {
            return await _context.DiscussionPosts
                .Where(p => p.InventoryId == inventoryId)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new DiscussionPostDto
                {
                    Id = p.Id,
                    UserName = p.UserName,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        // -----------------------------------------------------------------------
        // AddPostAsync
        // Saves the message, resolves the username, and returns the DTO.
        // The DTO is what the Hub broadcasts — never the raw entity.
        //
        // WHY RESOLVE USERNAME HERE?
        // The Hub only has the userId from the ClaimsPrincipal.
        // Resolving the display name in the service keeps that logic out of the Hub
        // and ensures the denormalised UserName is always set correctly.
        // -----------------------------------------------------------------------
        public async Task<DiscussionPostDto> AddPostAsync(Guid inventoryId, string userId, string content)
        {
            // Resolve the display name from Identity
            var user = await _userManager.FindByIdAsync(userId);
            var userName = user?.UserName ?? "Unknown";

            var post = new DiscussionPost
            {
                Id = Guid.NewGuid(),
                InventoryId = inventoryId,
                UserId = userId,
                UserName = userName,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.DiscussionPosts.Add(post);
            await _context.SaveChangesAsync();

            // Map to DTO — this is what gets sent over the wire to all clients
            return new DiscussionPostDto
            {
                Id = post.Id,
                UserName = post.UserName,
                Content = post.Content,
                CreatedAt = post.CreatedAt
            };
        }
    }
}
