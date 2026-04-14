using Microsoft.AspNetCore.Identity;

namespace Staff.Data;

public static class AppIdentitySeeder
{
    public const string AdminEmail = "admin@example.com";
    public const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in new[] { AppRoles.Admin, AppRoles.Hr, AppRoles.Lawyer })
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            await roleManager.CreateAsync(new Role
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });
        }

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Не удалось создать стартового администратора: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
        }
    }
}
