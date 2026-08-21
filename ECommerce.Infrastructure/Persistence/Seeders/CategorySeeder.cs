using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class CategorySeeder
{
    public static async Task<List<Category>> SeedAsync(ApplicationDbContext context, string actorId, ILogger logger)
    {
        var existing = await context.Categories.ToListAsync();
        var existingSlugs = existing.ToDictionary(c => c.Slug, StringComparer.OrdinalIgnoreCase);

        var topLevelSpecs = new (string Name, string Slug)[]
        {
            ("Electronics", "electronics"),
            ("Fashion & Apparel", "fashion"),
            ("Home & Living", "home-living"),
            ("Beauty & Personal Care", "beauty"),
            ("Sports & Fitness", "sports-fitness"),
            ("Photography & Gadgets", "gadgets")
        };

        var topAdded = false;
        foreach (var (name, slug) in topLevelSpecs)
        {
            if (!existingSlugs.ContainsKey(slug))
            {
                var cat = Category.Create(name, slug, null, actorId);
                context.Categories.Add(cat);
                existing.Add(cat);
                existingSlugs[slug] = cat;
                topAdded = true;
            }
        }

        if (topAdded)
        {
            await context.SaveChangesAsync();
        }

        var electronicsId = existingSlugs["electronics"].Id;
        var fashionId = existingSlugs["fashion"].Id;
        var homeId = existingSlugs["home-living"].Id;
        var beautyId = existingSlugs["beauty"].Id;
        var sportsId = existingSlugs["sports-fitness"].Id;
        var gadgetsId = existingSlugs["gadgets"].Id;

        var subSpecs = new (string Name, string Slug, Guid ParentId)[]
        {
            ("Smartphones", "smartphones", electronicsId),
            ("Laptops & Computers", "laptops-computers", electronicsId),
            ("Audio & Headphones", "audio-headphones", electronicsId),
            ("Smartwatches", "smartwatches", electronicsId),
            ("Men's Clothing", "mens-clothing", fashionId),
            ("Women's Clothing", "womens-clothing", fashionId),
            ("Footwear", "footwear", fashionId),
            ("Kitchen & Dining", "kitchen-dining", homeId),
            ("Bedding & Decor", "bedding-decor", homeId),
            ("Skincare & Treatments", "skincare", beautyId),
            ("Fragrances & Perfumes", "fragrances", beautyId),
            ("Fitness & Gym Gear", "fitness-gear", sportsId),
            ("Digital Cameras", "digital-cameras", gadgetsId)
        };

        var subAdded = false;
        foreach (var (name, slug, parentId) in subSpecs)
        {
            if (!existingSlugs.ContainsKey(slug))
            {
                var cat = Category.Create(name, slug, parentId, actorId);
                context.Categories.Add(cat);
                existing.Add(cat);
                existingSlugs[slug] = cat;
                subAdded = true;
            }
        }

        if (subAdded)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded new categories to reach {Count} total", existing.Count);
        }

        return await context.Categories.ToListAsync();
    }
}
