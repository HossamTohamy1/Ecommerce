using ECommerce.Domain.Entities;
using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class AdminUserSeeder
{
    public const string AdminEmail = "admin@ecommerce.local";
    public const string AdminPassword = "Admin@12345";

    public static async Task<ApplicationUser> SeedAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(AdminEmail);
        if (existing is not null)
        {
            return existing;
        }

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true,
            FullName = "Lumaire Administrator",
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, AdminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, AppConstants.Roles.Admin);
            logger.LogInformation("Seeded Admin user {Email}", AdminEmail);
        }
        else
        {
            logger.LogWarning("Failed to seed Admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return admin;
    }
}
