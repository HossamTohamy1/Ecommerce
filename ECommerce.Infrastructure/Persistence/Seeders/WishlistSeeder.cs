using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class WishlistSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        List<ApplicationUser> customers,
        List<Product> products,
        string actorId,
        ILogger logger)
    {
        var existingWishlists = await context.Wishlists.ToListAsync();
        var existingUserIds = existingWishlists.Select(w => w.UserId).ToHashSet();

        var wishlists = new List<Wishlist>();
        for (int i = 0; i < customers.Count; i++)
        {
            var customer = customers[i];
            if (existingUserIds.Contains(customer.Id.ToString()))
            {
                continue;
            }

            var w = Wishlist.Create(customer.Id.ToString(), customer.Id.ToString());

            var p1 = products[i % products.Count];
            var p2 = products[(i + 2) % products.Count];
            w.AddItem(p1.Id, customer.Id.ToString());
            w.AddItem(p2.Id, customer.Id.ToString());

            wishlists.Add(w);
        }

        if (wishlists.Count > 0)
        {
            context.Wishlists.AddRange(wishlists);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} customer wishlists", wishlists.Count);
        }
    }
}
