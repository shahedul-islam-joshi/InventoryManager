using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManager.Data;
using InventoryManager.Models.Domain;
using InventoryManager.Models.ViewModels;
using InventoryManager.Services.Interfaces;
using InventoryManager.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace InventoryManager.Controllers
{
    // WHY CONTROLLERS STAY THIN:
    // Controllers handle HTTP concerns only: routing, model binding, redirects, and HTTP status codes.
    // All business/permission logic lives in AccessService.
    // This separation means permission rules can change without touching controller code.
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccessService _accessService;
        private readonly IDiscussionService _discussionService;
        private readonly UserManager<ApplicationUser> _userManager;

        // IAccessService is injected — the controller never instantiates it directly.
        // This follows the Dependency Inversion principle and keeps the controller testable.
        public InventoryController(ApplicationDbContext context, IAccessService accessService, IDiscussionService discussionService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _accessService = accessService;
            _discussionService = discussionService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var inventories = await _context.Inventories.ToListAsync();
            return View(inventories);
        }

        // GET: Inventory/Details/5
        // Builds the InventoryDetailsViewModel with ownership and access flags
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var inventory = await _context.Inventories.FirstOrDefaultAsync(m => m.Id == id);
            if (inventory == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // PermissionHelper.IsOwner keeps the ownership check in one place
            bool isOwner = PermissionHelper.IsOwner(inventory, userId);

            // ADMINS ACT AS OWNERS:
            // High-privilege users must see the inventory exactly as the owner does.
            if (!isOwner)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    isOwner = true;
                }
            }

            // CanEditItems delegates to AccessService — owner OR granted user returns true
            bool canEdit = _accessService.CanEditItems(inventory.Id, userId);

            var items = await _context.Items
                .Where(i => i.InventoryId == id)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            // Only load the access list when the current user is the owner.
            // Non-owners never see the Access tab, so there is no need to query it for them.
            var usersWithAccess = isOwner
                ? await _accessService.GetUsersWithAccessAsync(inventory.Id)
                : new List<ApplicationUser>();

            // Load discussion posts for server-side rendering (guests can read without JS)
            var discussionPosts = await _discussionService.GetPostsAsync(inventory.Id);

            var viewModel = new InventoryDetailsViewModel
            {
                Inventory = inventory,
                IsOwner = isOwner,
                CanEdit = canEdit,
                Items = items,
                UsersWithAccess = usersWithAccess,
                DiscussionPosts = discussionPosts
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            inventory.Id = Guid.NewGuid();
            inventory.CreatedAt = DateTime.UtcNow;
            inventory.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ModelState.Remove("Id");
            ModelState.Remove("OwnerId");
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(x => x.Id == id);

            if (inventory != null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // Only the owner can delete an inventory — access grants do not include deletion
                if (!PermissionHelper.IsOwner(inventory, userId))
                    return Forbid();

                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST: Inventory/GrantAccess
        // Grants write access to a user by email.
        //
        // WHY OWNER-ONLY CHECK IN CONTROLLER?
        // The controller is responsible for verifying the HTTP request is authorised
        // (i.e., the requester is the owner). The actual grant logic lives in AccessService.
        // -----------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> GrantAccess(Guid inventoryId, string email)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);
            if (inventory == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Only the owner can manage access — return 403 for anyone else
            if (!PermissionHelper.IsOwner(inventory, userId))
                return Forbid();

            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    await _accessService.GrantAccessAsync(inventoryId, email);
                    TempData["AccessMessage"] = $"Access granted to {email}.";
                }
                catch (InvalidOperationException ex)
                {
                    // Surface the service error (user not found, already granted) to the view
                    TempData["AccessError"] = ex.Message;
                }
            }

            // Redirect back to Details, opening the Access tab
            return RedirectToAction(nameof(Details), new { id = inventoryId });
        }

        // -----------------------------------------------------------------------
        // POST: Inventory/RemoveAccess
        // Revokes write access for a specific user.
        // Only the owner can call this action.
        // -----------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> RemoveAccess(Guid inventoryId, string targetUserId)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);
            if (inventory == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Only the owner can remove access
            if (!PermissionHelper.IsOwner(inventory, userId))
                return Forbid();

            await _accessService.RemoveAccessAsync(inventoryId, targetUserId);
            TempData["AccessMessage"] = "Access removed.";

            return RedirectToAction(nameof(Details), new { id = inventoryId });
        }

        // -----------------------------------------------------------------------
        // GET: Inventory/Edit/5
        // Shows the edit form pre-filled with current inventory data.
        // Only the owner can edit inventory metadata.
        // -----------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.InventoryTags).ThenInclude(it => it.Tag)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (!PermissionHelper.IsOwner(inventory, userId)) return Forbid();

            var vm = new InventoryManager.Models.ViewModels.InventoryEditVM
            {
                Id = inventory.Id,
                Title = inventory.Title,
                Description = inventory.Description,
                Category = inventory.Category,
                ImageUrl = inventory.ImageUrl,
                IsPublic = inventory.IsPublic,
                Tags = string.Join(", ", inventory.InventoryTags.Select(it => it.Tag.Name)),
                Version = inventory.Version
            };

            return View(vm);
        }

        // -----------------------------------------------------------------------
        // POST: Inventory/Edit/5
        // -----------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Edit(InventoryManager.Models.ViewModels.InventoryEditVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == vm.Id);
            if (inventory == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (!PermissionHelper.IsOwner(inventory, userId)) return Forbid();

            inventory.Title = vm.Title;
            inventory.Description = vm.Description;
            inventory.Category = vm.Category;
            inventory.ImageUrl = vm.ImageUrl;
            inventory.IsPublic = vm.IsPublic;

            try
            {
                _context.Entry(inventory).Property(i => i.Version).OriginalValue = vm.Version;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "This inventory was modified by someone else. Please reload and try again.");
                return View(vm);
            }

            return RedirectToAction(nameof(Details), new { id = vm.Id });
        }
        // -----------------------------------------------------------------------
        // POST: Inventory/AutoSave
        // Background auto-save endpoint called by inventory-autosave.js.
        //
        // Returns:
        //   200 OK  { version: "<base64 rowversion>" }  — save succeeded
        //   409 Conflict                                — optimistic lock failure
        //   400 Bad Request                             — model invalid
        //   403 Forbidden                               — not the owner
        //
        // WHY RETURN THE NEW version?
        // EF rowversion is updated by the database on every write.
        // The JS must store the latest value and send it on the next auto-save,
        // otherwise every subsequent call would look like a stale-data conflict.
        //
        // WHY VALIDATE ANTIFORGERY TOKEN?
        // AutoSave is a mutating POST endpoint. Without this attribute a CSRF
        // attack could silently overwrite inventory data.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoSave(InventoryManager.Models.ViewModels.InventoryEditVM vm)
        {
            // Remove Tags from model state — the auto-save JS does not send it,
            // so model binding leaves it null, which would fail Required-like checks.
            ModelState.Remove(nameof(vm.Tags));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == vm.Id);
            if (inventory == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (!PermissionHelper.IsOwner(inventory, userId)) return Forbid();

            inventory.Title       = vm.Title;
            inventory.Description = vm.Description;
            inventory.Category    = vm.Category;
            inventory.ImageUrl    = vm.ImageUrl;
            inventory.IsPublic    = vm.IsPublic;

            try
            {
                // Apply the rowversion from the client so EF can detect if another
                // save happened between the page load and this auto-save request.
                _context.Entry(inventory).Property(i => i.Version).OriginalValue = vm.Version;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // 409 tells the JS to stop auto-saving and display the conflict message
                return Conflict();
            }

            // Return the new rowversion as a base64 string so the JS can update
            // the hidden Version field for the next auto-save cycle.
            var newVersion = Convert.ToBase64String(inventory.Version!);
            return Ok(new { version = newVersion });
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(Guid id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (inventory == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isOwner = PermissionHelper.IsOwner(inventory, userId);
            if (!isOwner)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    isOwner = true;
                }
            }
            bool canRead = inventory.IsPublic || isOwner || _accessService.CanEditItems(inventory.Id, userId);
            if (!canRead) return Forbid();

            var items = await _context.Items
                .Include(i => i.Inventory)
                .ThenInclude(inv => inv!.InventoryTags)
                .ThenInclude(it => it.Tag)
                .Include(i => i.ItemLikes)
                .Where(i => i.InventoryId == id)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var fields = inventory.Fields.OrderBy(f => f.Order).ToList();

            using var memoryStream = new System.IO.MemoryStream();
            using var writer = new System.IO.StreamWriter(memoryStream);
            using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            csv.WriteField("ID");
            csv.WriteField("Name");
            csv.WriteField("Description");
            foreach (var field in fields)
            {
                csv.WriteField(field.Title);
            }
            csv.WriteField("Tags");
            csv.WriteField("Likes");
            csv.WriteField("Created At");
            csv.NextRecord();

            foreach (var item in items)
            {
                csv.WriteField(item.CustomId ?? item.Id.ToString());
                csv.WriteField(item.Name);
                csv.WriteField(item.Description);

                foreach (var field in fields)
                {
                    string? value = field.FieldType switch
                    {
                        "Text" => field.SlotIndex == 1 ? item.Text1 : field.SlotIndex == 2 ? item.Text2 : item.Text3,
                        "Number" => field.SlotIndex == 1 ? item.Number1?.ToString() : field.SlotIndex == 2 ? item.Number2?.ToString() : item.Number3?.ToString(),
                        "Bool" => field.SlotIndex == 1 ? item.Bool1?.ToString() : field.SlotIndex == 2 ? item.Bool2?.ToString() : item.Bool3?.ToString(),
                        "Date" => field.SlotIndex == 1 ? item.Date1?.ToString("yyyy-MM-dd") : field.SlotIndex == 2 ? item.Date2?.ToString("yyyy-MM-dd") : item.Date3?.ToString("yyyy-MM-dd"),
                        _ => ""
                    };
                    csv.WriteField(value ?? "");
                }

                var tags = string.Join(", ", item.Inventory?.InventoryTags.Select(it => it.Tag.Name) ?? Array.Empty<string>());
                csv.WriteField(tags);
                csv.WriteField(item.ItemLikes.Count.ToString());
                csv.WriteField(item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                csv.NextRecord();
            }

            writer.Flush();
            return File(memoryStream.ToArray(), "text/csv", $"Inventory_{inventory.Title}_Items.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(Guid id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (inventory == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isOwner = PermissionHelper.IsOwner(inventory, userId);
            if (!isOwner)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    isOwner = true;
                }
            }
            bool canRead = inventory.IsPublic || isOwner || _accessService.CanEditItems(inventory.Id, userId);
            if (!canRead) return Forbid();

            var items = await _context.Items
                .Include(i => i.Inventory)
                .ThenInclude(inv => inv!.InventoryTags)
                .ThenInclude(it => it.Tag)
                .Include(i => i.ItemLikes)
                .Where(i => i.InventoryId == id)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var fields = inventory.Fields.OrderBy(f => f.Order).ToList();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Items");

            int col = 1;
            worksheet.Cell(1, col++).Value = "ID";
            worksheet.Cell(1, col++).Value = "Name";
            worksheet.Cell(1, col++).Value = "Description";
            foreach (var field in fields)
            {
                worksheet.Cell(1, col++).Value = field.Title;
            }
            worksheet.Cell(1, col++).Value = "Tags";
            worksheet.Cell(1, col++).Value = "Likes";
            worksheet.Cell(1, col++).Value = "Created At";

            int row = 2;
            foreach (var item in items)
            {
                col = 1;
                worksheet.Cell(row, col++).Value = item.CustomId ?? item.Id.ToString();
                worksheet.Cell(row, col++).Value = item.Name;
                worksheet.Cell(row, col++).Value = item.Description;

                foreach (var field in fields)
                {
                    string? value = field.FieldType switch
                    {
                        "Text" => field.SlotIndex == 1 ? item.Text1 : field.SlotIndex == 2 ? item.Text2 : item.Text3,
                        "Number" => field.SlotIndex == 1 ? item.Number1?.ToString() : field.SlotIndex == 2 ? item.Number2?.ToString() : item.Number3?.ToString(),
                        "Bool" => field.SlotIndex == 1 ? item.Bool1?.ToString() : field.SlotIndex == 2 ? item.Bool2?.ToString() : item.Bool3?.ToString(),
                        "Date" => field.SlotIndex == 1 ? item.Date1?.ToString("yyyy-MM-dd") : field.SlotIndex == 2 ? item.Date2?.ToString("yyyy-MM-dd") : item.Date3?.ToString("yyyy-MM-dd"),
                        _ => ""
                    };
                    worksheet.Cell(row, col++).Value = value ?? "";
                }

                var tags = string.Join(", ", item.Inventory?.InventoryTags.Select(it => it.Tag.Name) ?? Array.Empty<string>());
                worksheet.Cell(row, col++).Value = tags;
                worksheet.Cell(row, col++).Value = item.ItemLikes.Count;
                worksheet.Cell(row, col++).Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var memoryStream = new System.IO.MemoryStream();
            workbook.SaveAs(memoryStream);
            return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Inventory_{inventory.Title}_Items.xlsx");
        }
    }
}
