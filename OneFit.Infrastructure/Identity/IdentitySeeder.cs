
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OneFit.Infrastructure.Constants;

namespace OneFit.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed roles
        foreach (var role in AppRoles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"✓ Role '{role}' created successfully.");
            }
        }

        // Create default admin user
        var adminEmail = "admin@onefit.com";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "OneFit",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(
                newAdmin,
                "Admin@123456"
            );

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    newAdmin,
                    AppRoles.Admin
                );

                Console.WriteLine(
                    $"✓ Default admin user '{adminEmail}' created."
                );
            }
            else
            {
                Console.WriteLine(
                    $"✗ Failed to create admin user: " +
                    $"{string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }
        }
    }
}

