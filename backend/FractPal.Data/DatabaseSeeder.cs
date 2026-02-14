namespace FractPal.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopeProvider = scope.ServiceProvider;
        var dbContext = scopeProvider.GetRequiredService<ApplicationDbContext>();

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        if (!userManager.Users.Any())
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
        }
    }

    private static async Task SeedUsers(UserManager<IdentityUser> userManager)
    {
        var adminUser = new IdentityUser
        {
            UserName = "Lindenmayer",
            Email = "admin@admin.com",
            EmailConfirmed = true
        };

        string adminPassword = "Admin#123";

        await SeedUser(adminUser, adminPassword, "Admin", userManager);

        var user = new IdentityUser
        {
            UserName = "Koch",
            Email = "user@user.com",
            EmailConfirmed = true
        };

        string userPassword = "User#123";

        await SeedUser(user, userPassword, "User", userManager);
    }

    private static async Task SeedUser(IdentityUser user, string password, string roleName,
        UserManager<IdentityUser> userManager)
    {
        if (user.Email is null)
        {
            return;
        }

        var userInfo = await userManager.FindByEmailAsync(user.Email);
        if (userInfo == null)
        {
            var created = await userManager
                .CreateAsync(user, password);
            if (created.Succeeded)
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "User" };

        foreach (var role in roleNames)
        {
            bool roleExist = await roleManager.RoleExistsAsync(role);

            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
