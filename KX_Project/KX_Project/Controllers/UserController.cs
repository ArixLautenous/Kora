using KX_Project.Models;
using KX_Project.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KX_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var viewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user),
                    IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now
                };
                userRolesViewModel.Add(viewModel);
            }

            return View(userRolesViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Prevent admin from locking themselves
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser.Id) return BadRequest("Không thể tự khóa tài khoản của mình.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now)
            {
                // Unlock
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                // Lock for 100 years
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Prevent admin from demoting themselves
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser.Id && newRole != "Admin") 
                return BadRequest("Không thể tự tước quyền Admin của chính mình.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction(nameof(Index));
        }
    }
}
