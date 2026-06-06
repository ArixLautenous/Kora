using Microsoft.AspNetCore.Identity;

namespace KX_Project.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsLockedOut { get; set; }
    }
}
