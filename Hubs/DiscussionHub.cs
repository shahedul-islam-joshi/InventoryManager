using InventoryManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace InventoryManager.Hubs
{
    // DiscussionHub handles real-time messaging for inventory discussions.
    //
    // WHY DOES THE HUB NOT ACCESS DbContext DIRECTLY?
    // Hubs are transient — a new instance is created for every connection event.
    // ApplicationDbContext is scoped (one per HTTP request/connection lifetime).
    // Injecting a scoped DbContext into a transient Hub causes a DI lifetime
    // mismatch and can lead to concurrency bugs or disposed-context exceptions.
    // Instead, the Hub delegates all persistence to IDiscussionService,
    // which is resolved correctly as a scoped service via DI.
    //
    // WHY USE GROUPS?
    // Each inventory has its own discussion. Groups let us broadcast a message
    // only to clients viewing the same inventory, rather than to every connected user.
    // Group name = inventoryId.ToString() — simple and unique.
    public class DiscussionHub : Hub
    {
        private readonly IDiscussionService _discussionService;

        public DiscussionHub(IDiscussionService discussionService)
        {
            _discussionService = discussionService;
        }

        // -----------------------------------------------------------------------
        // JoinInventoryGroup
        // Called by the client immediately after connecting.
        // Adds the connection to a group named after the inventory ID so that
        // only messages for this inventory are received.
        // -----------------------------------------------------------------------
        public async Task JoinInventoryGroup(Guid inventoryId)
        {
            // Group name is the inventory ID — unique and easy to derive on the client
            await Groups.AddToGroupAsync(Context.ConnectionId, inventoryId.ToString());
        }

        // -----------------------------------------------------------------------
        // SendMessage
        // Called by authenticated clients to post a new message.
        // The [Authorize] attribute ensures guests cannot call this method.
        //
        // Flow:
        //   1. Resolve the current user's ID from the Hub context.
        //   2. Delegate persistence + DTO mapping to IDiscussionService.
        //   3. Broadcast the DTO to all clients in the inventory group.
        // -----------------------------------------------------------------------
        [Authorize]
        public async Task SendMessage(Guid inventoryId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            // Get the authenticated user's ID from the ClaimsPrincipal
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return;

            // Persist and map to DTO — all DB logic stays in the service
            var dto = await _discussionService.AddPostAsync(inventoryId, userId, message.Trim());

            // Broadcast to everyone in the inventory group (including the sender)
            // WHY Clients.Group() AND NOT Clients.All()?
            // We only want users viewing this specific inventory to receive the message.
            await Clients.Group(inventoryId.ToString())
                         .SendAsync("receiveMessage", dto);
        }
    }
}
