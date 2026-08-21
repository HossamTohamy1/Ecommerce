using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class DiscountSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        List<Product> products,
        string actorId,
        ILogger logger)
    {
        var existing = await context.Discounts.Include(d => d.ProductDiscounts).ToListAsync();
        var existingCodes = existing.Where(d => d.Code != null).ToDictionary(d => d.Code!, StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;
        var discountSpecs = new (string Name, string Code, DiscountType Type, decimal Value, decimal? MinAmount, int? Limit)[]
        {
            ("Welcome New Customer 10%", "WELCOME10", DiscountType.Percentage, 10m, 1000m, 500),
            ("Summer Mega Sale 20%", "SUMMER20", DiscountType.Percentage, 20m, 2500m, 200),
            ("Flash Deal 500 EGP Off", "FLASH500", DiscountType.FixedAmount, 500m, 3000m, 100),
            ("Tech Gadgets Special 15%", "TECH15", DiscountType.Percentage, 15m, 5000m, 150),
            ("Lumaire VIP Club 25%", "VIP25", DiscountType.Percentage, 25m, 10000m, 50),
            ("Weekend Special 300 EGP", "WEEKEND300", DiscountType.FixedAmount, 300m, 1500m, 300),
            ("Eid Mubarak Celebration 15%", "EID15", DiscountType.Percentage, 15m, 2000m, 250),
            ("Black Friday Mega 30%", "BLACKFRIDAY30", DiscountType.Percentage, 30m, 5000m, 1000),
            ("Cyber Monday Deal 1000 EGP", "CYBER1000", DiscountType.FixedAmount, 1000m, 8000m, 100),
            ("Fashion Week Promo 18%", "FASHION18", DiscountType.Percentage, 18m, 1200m, 200),
            ("Sports & Fitness Discount 12%", "FITNESS12", DiscountType.Percentage, 12m, 1500m, 150),
            ("Super Saver 750 EGP Off", "SAVER750", DiscountType.FixedAmount, 750m, 6000m, 75),
            ("Back to School 10%", "SCHOOL10", DiscountType.Percentage, 10m, 1000m, 400),
            ("Spring Refresh 15%", "SPRING15", DiscountType.Percentage, 15m, 2000m, 200),
            ("First App Order 250 EGP", "APPFIRST250", DiscountType.FixedAmount, 250m, 1000m, 500),
            ("Golden Anniversary 35%", "GOLDEN35", DiscountType.Percentage, 35m, 15000m, 30)
        };

        var added = false;
        int pIdx = 0;
        foreach (var spec in discountSpecs)
        {
            if (!existingCodes.ContainsKey(spec.Code))
            {
                var d = Discount.Create(
                    spec.Name,
                    spec.Code,
                    spec.Type,
                    spec.Value,
                    now.AddDays(-10),
                    now.AddDays(180),
                    spec.MinAmount,
                    spec.Limit,
                    actorId);

                if (products.Count > 0)
                {
                    var targetProduct = products[pIdx % products.Count];
                    d.AssignProduct(targetProduct.Id, actorId);
                    pIdx++;
                }

                context.Discounts.Add(d);
                existing.Add(d);
                existingCodes[spec.Code] = d;
                added = true;
            }
        }

        if (added)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded new discounts to reach {Count} total", existing.Count);
        }
    }
}
