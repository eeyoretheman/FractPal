namespace FractPal.Data;

using FractPal.Model.Entities;
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

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<FractPalUser>>();

        if (!userManager.Users.Any())
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
        }
    }

    private static async Task SeedUsers(UserManager<FractPalUser> userManager)
    {
        var adminUser = new FractPalUser
        {
            UserName = "Lindenmayer",
            Email = "admin@admin.com",
            EmailConfirmed = true
        };

        var adminPassword = "Admin#123";

        await SeedUser(adminUser, adminPassword, "Admin", userManager);

        var user = new FractPalUser
        {
            UserName = "Koch",
            Email = "user@user.com",
            EmailConfirmed = true
        };

        var userPassword = "User#123";

        await SeedUser(user, userPassword, "User", userManager);
    }

    private static async Task SeedUser(FractPalUser user, string password, string roleName,
        UserManager<FractPalUser> userManager)
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
    private static async Task SeedRoles(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roleNames = ["Admin", "User"];

        foreach (var role in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(role);

            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
