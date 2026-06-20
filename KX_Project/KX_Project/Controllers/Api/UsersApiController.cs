using KX_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KX_Project.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    // Yêu cầu quyền Admin để xem danh sách toàn bộ khách hàng
    [Authorize(Roles = "Admin")]
    public class UsersApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersApiController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/UsersApi
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.FullName,
                    u.PhoneNumber,
                    u.Address
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/UsersApi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                user.Address
            });
        }
    }
}
