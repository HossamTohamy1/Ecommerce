using ECommerce.Domain.Entities;
using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class CustomerSeeder
{
    public const string DefaultCustomerPassword = "Customer@12345";

    public static async Task<List<ApplicationUser>> SeedAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var customerData = new (string Email, string FullName)[]
        {
            ("customer@ecommerce.local", "Demo Customer"),
            ("ahmed.hassan@example.com", "Ahmed Hassan"),
            ("sarah.ibrahim@example.com", "Sarah Ibrahim"),
            ("omar.khalil@example.com", "Omar Khalil"),
            ("nour.ali@example.com", "Nour Ali"),
            ("tarek.sayed@example.com", "Tarek Sayed"),
            ("youssef.nabil@example.com", "Youssef Nabil"),
            ("layla.mahmoud@example.com", "Layla Mahmoud"),
            ("mona.gamal@example.com", "Mona Gamal"),
            ("karim.fawzy@example.com", "Karim Fawzy"),
            ("hoda.salem@example.com", "Hoda Salem"),
            ("amr.diab@example.com", "Amr Diab"),
            ("dina.adel@example.com", "Dina Adel"),
            ("mostafa.kamal@example.com", "Mostafa Kamal"),
            ("yasmin.sabry@example.com", "Yasmin Sabry"),
            ("khaled.zaki@example.com", "Khaled Zaki")
        };

        var users = new List<ApplicationUser>();
        foreach (var (email, fullName) in customerData)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                users.Add(existing);
                continue;
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-Random.Shared.Next(10, 90))
            };

            var res = await userManager.CreateAsync(user, DefaultCustomerPassword);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AppConstants.Roles.Customer);
                users.Add(user);
            }
        }

        logger.LogInformation("Customers ready: {Count} active customer accounts", users.Count);
        return users;
    }
}
