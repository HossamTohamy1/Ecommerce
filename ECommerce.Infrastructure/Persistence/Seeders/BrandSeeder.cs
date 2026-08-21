using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class BrandSeeder
{
    public static async Task<List<Brand>> SeedAsync(ApplicationDbContext context, string actorId, ILogger logger)
    {
        var existing = await context.Brands.ToListAsync();
        var existingNames = existing.ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase);

        var brandData = new (string Name, string LogoUrl)[]
        {
            ("Apple", "/assets/media/svg/brand-logos/apple-black.svg"),
            ("Samsung", "/assets/media/svg/brand-logos/smartphone.svg"),
            ("Sony", "/assets/media/svg/brand-logos/beats-electronics.svg"),
            ("Bose", "/assets/media/svg/brand-logos/bose.svg"),
            ("Dell", "/assets/media/svg/brand-logos/deloitte-1.svg"),
            ("HP", "/assets/media/svg/brand-logos/hp-2.svg"),
            ("Nike", "/assets/media/svg/brand-logos/nike-4.svg"),
            ("Adidas", "/assets/media/svg/brand-logos/adidas-9.svg"),
            ("Puma", "/assets/media/svg/brand-logos/puma-logo.svg"),
            ("Zara", "/assets/media/svg/brand-logos/brantano.svg"),
            ("Levi's", "/assets/media/svg/brand-logos/levis.svg"),
            ("Canon", "/assets/media/svg/brand-logos/canon-logo.svg"),
            ("Casio", "/assets/media/svg/brand-logos/casio.svg"),
            ("Philips", "/assets/media/svg/brand-logos/general-electric.svg"),
            ("L'Oreal", "/assets/media/svg/brand-logos/unilever-2.svg"),
            ("Garmin", "/assets/media/svg/brand-logos/garmin-1.svg")
        };

        var added = false;
        foreach (var (name, logoUrl) in brandData)
        {
            if (!existingNames.ContainsKey(name))
            {
                var b = Brand.Create(name, actorId);
                b.SetLogo(logoUrl, actorId);
                context.Brands.Add(b);
                existing.Add(b);
                existingNames[name] = b;
                added = true;
            }
        }

        if (added)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded new brands to reach {Count} total", existing.Count);
        }

        return await context.Brands.ToListAsync();
    }
}
