using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManager.Models.Domain;

namespace InventoryManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> BlockUsers(List<string> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    user.IsBlocked = true;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUsers(List<string> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    user.IsBlocked = false;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUsers(List<string> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmins(List<string> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAdmins(List<string> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await _userManager.RemoveFromRoleAsync(user, "Admin");
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
