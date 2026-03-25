using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using InventoryManager.Models.DTOs;
using InventoryManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers
{
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class SupportController : Controller
    {
        private readonly DropboxService _dropboxService;

        public SupportController(DropboxService dropboxService)
        {
            _dropboxService = dropboxService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTicket()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                var summary = root.TryGetProperty("summary", out var s) ? s.GetString() : "";
                var priority = root.TryGetProperty("priority", out var p) ? p.GetString() : "";
                var inventoryTitle = root.TryGetProperty("inventoryTitle", out var i) ? i.GetString() : null;
                var currentPageLink = root.TryGetProperty("currentPageLink", out var c) ? c.GetString() : "";

                var ticket = new SupportTicketDto
                {
                    ReportedBy = User.Identity?.Name ?? "Anonymous",
                    Inventory = inventoryTitle,
                    Link = currentPageLink,
                    Priority = priority,
                    Summary = summary,
                    AdminEmails = new List<string> { "admin@example.com" }
                };

                await _dropboxService.UploadTicketAsync(ticket);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
