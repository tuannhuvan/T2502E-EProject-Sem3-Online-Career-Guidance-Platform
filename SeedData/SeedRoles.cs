
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.SeedData;

public static class SeedRoles
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<User>>();

        string[] roles =
        {
            "Admin",
            "User",
            "Menter",
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }

        // Tạo tài khoản Admin mặc định
        const string email = "Dungntth@gmail.com";
        const string password = "Mothaiba123@@";
        const string fullname = "MguyenTuanDung";

        var admin =
            await userManager.FindByEmailAsync(email);

        if (admin == null)
        {
            admin = new User
            {
                UserName = fullname,
                Email = email,
                FullName = "Administrator",
                EmailConfirmed = true
            };

            var result =
                await userManager.CreateAsync(
                    admin,
                    password);

            if (!result.Succeeded)
            {
                Console.WriteLine("===== CREATE ADMIN ERROR =====");

                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
            else
            {
                Console.WriteLine("===== ADMIN CREATED =====");

                var roleResult =
                    await userManager.AddToRoleAsync(
                        admin,
                        "Admin");

                if (!roleResult.Succeeded)
                {
                    Console.WriteLine("===== ADD ROLE ERROR =====");

                    foreach (var error in roleResult.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }
            }
        }
        
    }
}
