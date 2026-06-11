using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BanNoiThat.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (await userManager.FindByEmailAsync("admin@webnoithat.com") == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = "admin@webnoithat.com",
                    Email = "admin@webnoithat.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}