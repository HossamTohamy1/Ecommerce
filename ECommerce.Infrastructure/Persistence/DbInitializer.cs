using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("Beginning DbInitializer execution...");

            await RoleSeeder.SeedAsync(roleManager, logger);
            var admin = await AdminUserSeeder.SeedAsync(userManager, logger);
            var customers = await CustomerSeeder.SeedAsync(userManager, logger);
            var actorId = admin.Id.ToString();

            await AddressSeeder.SeedAsync(context, customers, actorId, logger);
            var categories = await CategorySeeder.SeedAsync(context, actorId, logger);
            var brands = await BrandSeeder.SeedAsync(context, actorId, logger);
            var products = await ProductSeeder.SeedAsync(context, categories, brands, actorId, logger);
            await DiscountSeeder.SeedAsync(context, products, actorId, logger);
            await OrderSeeder.SeedAsync(context, customers, products, actorId, logger);
            await ReviewSeeder.SeedAsync(context, customers, products, actorId, logger);
            await WishlistSeeder.SeedAsync(context, customers, products, actorId, logger);
            await NotificationSeeder.SeedAsync(context, admin, customers, actorId, logger);

            logger.LogInformation("DbInitializer executed all seed modules successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "FATAL: DbInitializer failed during execution. Inner: {InnerMessage}", ex.InnerException?.Message ?? ex.Message);
            throw;
        }
    }
}
