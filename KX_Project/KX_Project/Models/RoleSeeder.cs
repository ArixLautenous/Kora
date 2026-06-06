using Microsoft.AspNetCore.Identity;

namespace KX_Project.Models
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Staff", "Customer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Tạo tài khoản Admin mặc định
            var adminUser = await userManager.FindByEmailAsync("admin@kora.com");
            if (adminUser == null)
            {
                var user = new ApplicationUser { UserName = "admin@kora.com", Email = "admin@kora.com" };
                var createPowerUser = await userManager.CreateAsync(user, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Tạo tài khoản Staff mặc định
            var staffUser = await userManager.FindByEmailAsync("staff@kora.com");
            if (staffUser == null)
            {
                var user = new ApplicationUser { UserName = "staff@kora.com", Email = "staff@kora.com" };
                var createPowerUser = await userManager.CreateAsync(user, "Staff@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Staff");
                }
            }

            // Tạo tài khoản Customer mặc định
            var customerUser = await userManager.FindByEmailAsync("customer@kora.com");
            if (customerUser == null)
            {
                var user = new ApplicationUser { UserName = "customer@kora.com", Email = "customer@kora.com" };
                var createPowerUser = await userManager.CreateAsync(user, "Customer@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }
            }
        }
    }
}
