using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KX_Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
    }
}
