using BasicAuthDemo.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BasicAuthDemo.DbContexts;

public static class Seed
{
    public static async Task SeedUsers(
        UserManager<UserEntity> userManager,
        RoleManager<UserRole> roleManager)
    {
        if (await userManager.Users.AnyAsync())
        {
            return;
        }


        var roles = new List<UserRole>
        {
            new UserRole
            {
                Name = "Client"
            },
            new UserRole
            {
                Name = "Admin"
            },
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        var admin = new UserEntity
        {
            FirstName = "admin",
            LastName = "admin",
            Email = "admin@test.com",
            UserName = "admin@test.com"
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, new[] { "Admin", "Client" });
    }
}
