using InventoryManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManager.Controllers
{
    // DiscussionController provides REST fallback endpoints for the discussion feature.
    // Real-time messaging is primarily handled by DiscussionHub (SignalR).
    // These REST endpoints serve as a non-JS fallback and for testing.
    [Authorize]
    public class DiscussionController : Controller
    {
        private readonly IDiscussionService _discussionService;

        public DiscussionController(IDiscussionService discussionService)
        {
            _discussionService = discussionService;
        }

        // GET: /Discussion/Posts?inventoryId=...
        // Returns all discussion posts for an inventory as JSON.
        [HttpGet]
        public async Task<IActionResult> Posts(Guid inventoryId)
        {
            var posts = await _discussionService.GetPostsAsync(inventoryId);
            return Json(posts);
        }

        // POST: /Discussion/Post
        // Posts a new discussion message (REST fallback — SignalR is preferred).
        [HttpPost]
        public async Task<IActionResult> Post(Guid inventoryId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var userName = User.Identity?.Name ?? "User";

            await _discussionService.AddPostAsync(inventoryId, userId, message);
            return RedirectToAction("Details", "Inventory", new { id = inventoryId });
        }
    }
}
