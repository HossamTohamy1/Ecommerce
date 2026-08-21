using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class ProductSeeder
{
    public static async Task<List<Product>> SeedAsync(
        ApplicationDbContext context,
        List<Category> categories,
        List<Brand> brands,
        string actorId,
        ILogger logger)
    {
        var existing = await context.Products.Include(p => p.Images).Include(p => p.Variants).ToListAsync();
        var existingSkus = existing.ToDictionary(p => p.SKU.Trim(), StringComparer.OrdinalIgnoreCase);
        var existingVariantSkus = await context.ProductVariants.Select(v => v.SKU.Trim()).ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        var catMap = categories.ToDictionary(c => c.Slug, c => c.Id, StringComparer.OrdinalIgnoreCase);
        var brandMap = brands.ToDictionary(b => b.Name, b => b.Id, StringComparer.OrdinalIgnoreCase);

        Guid GetCat(string slug) => catMap.TryGetValue(slug, out var id) ? id : categories[0].Id;
        Guid GetBrand(string name) => brandMap.TryGetValue(name, out var id) ? id : brands[0].Id;

        var productSpecs = new[]
        {
            new {
                Name = "iPhone 16 Pro Max 256GB",
                Desc = "Titanium design with A18 Pro chip, 48MP Fusion camera system, and industry-leading battery life.",
                Sku = "IPHONE-16-PRO-MAX",
                Price = 64999.00m,
                CompareAt = 69999.00m,
                Stock = 25,
                CategorySlug = "smartphones",
                BrandName = "Apple",
                Image = "/images/seed/products/iphone-16-pro.svg",
                Variants = new[] { ("128GB - Natural Titanium", "IPH-16-128-NAT", (decimal?)59999m, 10, "128GB", "Natural Titanium"), ("256GB - Black Titanium", "IPH-16-256-BLK", (decimal?)64999m, 15, "256GB", "Black Titanium") }
            },
            new {
                Name = "Samsung Galaxy S24 Ultra 512GB",
                Desc = "Galaxy AI-powered flagship with built-in S Pen, 200MP camera, and titanium armor frame.",
                Sku = "GALAXY-S24-ULTRA",
                Price = 58999.00m,
                CompareAt = 62999.00m,
                Stock = 20,
                CategorySlug = "smartphones",
                BrandName = "Samsung",
                Image = "/images/seed/products/galaxy-s24-ultra.svg",
                Variants = new[] { ("256GB - Titanium Gray", "S24U-256-GRY", (decimal?)53999m, 8, "256GB", "Titanium Gray"), ("512GB - Titanium Black", "S24U-512-BLK", (decimal?)58999m, 12, "512GB", "Titanium Black") }
            },
            new {
                Name = "Apple MacBook Pro 16\" M3 Max",
                Desc = "Liquid Retina XDR display, M3 Max chip with 16-core CPU and 40-core GPU, 36GB unified memory.",
                Sku = "MBP-16-M3-MAX",
                Price = 145000.00m,
                CompareAt = 155000.00m,
                Stock = 12,
                CategorySlug = "laptops-computers",
                BrandName = "Apple",
                Image = "/images/seed/products/macbook-pro-16.svg",
                Variants = new[] { ("Space Black - 36GB / 1TB", "MBP-16-36-1TB", (decimal?)145000m, 6, "16 Inch", "Space Black"), ("Silver - 48GB / 1TB", "MBP-16-48-1TB", (decimal?)159000m, 6, "16 Inch", "Silver") }
            },
            new {
                Name = "Dell XPS 15 9530 Core i9",
                Desc = "15.6\" OLED 3.5K touch display, Intel Core i9-13900H, 32GB RAM, 1TB SSD, NVIDIA RTX 4070.",
                Sku = "DELL-XPS-15-9530",
                Price = 89999.00m,
                CompareAt = 95000.00m,
                Stock = 15,
                CategorySlug = "laptops-computers",
                BrandName = "Dell",
                Image = "/images/seed/products/dell-xps-15.svg",
                Variants = new[] { ("FHD+ i7 - 16GB / 512GB", "DELL-XPS-I7", (decimal?)74999m, 7, "15.6 Inch", "Platinum Silver"), ("OLED i9 - 32GB / 1TB", "DELL-XPS-I9", (decimal?)89999m, 8, "15.6 Inch", "Platinum Silver") }
            },
            new {
                Name = "Sony WH-1000XM5 Wireless Headphones",
                Desc = "Industry-leading noise canceling with Auto NC Optimizer, crystal clear hands-free calling, and 30hr battery.",
                Sku = "SONY-WH1000XM5",
                Price = 17500.00m,
                CompareAt = 19999.00m,
                Stock = 30,
                CategorySlug = "audio-headphones",
                BrandName = "Sony",
                Image = "/images/seed/products/sony-wh1000xm5.svg",
                Variants = new[] { ("Black Edition", "SONY-XM5-BLK", (decimal?)17500m, 18, "Over-Ear", "Black"), ("Silver Edition", "SONY-XM5-SLV", (decimal?)17500m, 12, "Over-Ear", "Silver") }
            },
            new {
                Name = "Bose QuietComfort Ultra Headphones",
                Desc = "World-class active noise cancellation with breakthrough spatialized audio and custom tuned comfort.",
                Sku = "BOSE-QC-ULTRA",
                Price = 18999.00m,
                CompareAt = 21500.00m,
                Stock = 22,
                CategorySlug = "audio-headphones",
                BrandName = "Bose",
                Image = "/images/seed/products/bose-quietcomfort.svg",
                Variants = new[] { ("Black", "BOSE-QCU-BLK", (decimal?)18999m, 12, "Over-Ear", "Black"), ("White Smoke", "BOSE-QCU-WHT", (decimal?)18999m, 10, "Over-Ear", "White Smoke") }
            },
            new {
                Name = "Nike Air Max 270 Running Shoes",
                Desc = "Nike's biggest heel Air unit yet delivers ultra-soft cushioning that feels as unbelievable as it looks.",
                Sku = "NIKE-AIR-MAX-270",
                Price = 6499.00m,
                CompareAt = 7200.00m,
                Stock = 45,
                CategorySlug = "footwear",
                BrandName = "Nike",
                Image = "/images/seed/products/nike-air-max.svg",
                Variants = new[] { ("Size 42 - Black/White", "AM270-42-BLK", (decimal?)6499m, 15, "42", "Black/White"), ("Size 43 - Black/White", "AM270-43-BLK", (decimal?)6499m, 15, "43", "Black/White"), ("Size 44 - Triple Red", "AM270-44-RED", (decimal?)6499m, 15, "44", "Triple Red") }
            },
            new {
                Name = "Adidas Ultraboost Light Running Shoes",
                Desc = "Epic energy in every stride with light BOOST midsole and Primeknit+ upper engineered for maximum performance.",
                Sku = "ADIDAS-UB-LIGHT",
                Price = 7299.00m,
                CompareAt = 8100.00m,
                Stock = 40,
                CategorySlug = "footwear",
                BrandName = "Adidas",
                Image = "/images/seed/products/adidas-ultraboost.svg",
                Variants = new[] { ("Size 42 - Core Black", "UB-42-BLK", (decimal?)7299m, 12, "42", "Core Black"), ("Size 43 - Cloud White", "UB-43-WHT", (decimal?)7299m, 14, "43", "Cloud White"), ("Size 44 - Solar Red", "UB-44-RED", (decimal?)7299m, 14, "44", "Solar Red") }
            },
            new {
                Name = "Puma Velocity Nitro 3 Running Shoes",
                Desc = "NITRO foam cushioning for explosive responsiveness and durable PUMAGRIP outsole traction.",
                Sku = "PUMA-VELOCITY-N3",
                Price = 4999.00m,
                CompareAt = 5800.00m,
                Stock = 35,
                CategorySlug = "footwear",
                BrandName = "Puma",
                Image = "/images/seed/products/puma-velocity-nitro.svg",
                Variants = new[] { ("Size 42 - Fire Red", "PVN3-42-RED", (decimal?)4999m, 15, "42", "Fire Red"), ("Size 43 - Puma Black", "PVN3-43-BLK", (decimal?)4999m, 20, "43", "Puma Black") }
            },
            new {
                Name = "Zara Structured Oversized Blazer",
                Desc = "Tailored oversized blazer with peak lapels, double-breasted button fastening, and flap pockets.",
                Sku = "ZARA-BLAZER-OVERSIZED",
                Price = 3899.00m,
                CompareAt = 4500.00m,
                Stock = 28,
                CategorySlug = "mens-clothing",
                BrandName = "Zara",
                Image = "/images/seed/products/zara-oversized-blazer.svg",
                Variants = new[] { ("Size M - Charcoal", "ZARA-BLZ-M-CHR", (decimal?)3899m, 10, "M", "Charcoal"), ("Size L - Midnight Black", "ZARA-BLZ-L-BLK", (decimal?)3899m, 12, "L", "Midnight Black"), ("Size XL - Navy", "ZARA-BLZ-XL-NVY", (decimal?)3899m, 6, "XL", "Navy Blue") }
            },
            new {
                Name = "Levi's 501 Original Fit Jeans",
                Desc = "The blueprint for every pair of jeans in existence with signature button fly and iconic straight leg silhouette.",
                Sku = "LEVIS-501-ORIGINAL",
                Price = 3499.00m,
                CompareAt = 3999.00m,
                Stock = 50,
                CategorySlug = "mens-clothing",
                BrandName = "Levi's",
                Image = "/images/seed/products/levis-501-original.svg",
                Variants = new[] { ("32x32 - Stonewash", "L501-32-STONE", (decimal?)3499m, 20, "32x32", "Stonewash Blue"), ("34x32 - Dark Indigo", "L501-34-IND", (decimal?)3499m, 20, "34x32", "Dark Indigo"), ("36x32 - Black", "L501-36-BLK", (decimal?)3499m, 10, "36x32", "Black") }
            },
            new {
                Name = "Apple Watch Ultra 2 GPS + Cellular",
                Desc = "Rugged 49mm titanium case, precision dual-frequency GPS, and up to 36 hours of normal battery life.",
                Sku = "APPLE-WATCH-ULTRA-2",
                Price = 42500.00m,
                CompareAt = 46000.00m,
                Stock = 18,
                CategorySlug = "smartwatches",
                BrandName = "Apple",
                Image = "/images/seed/products/apple-watch-ultra.svg",
                Variants = new[] { ("Orange Ocean Band", "AWU2-ORG-OCN", (decimal?)42500m, 9, "49mm", "Orange"), ("Blue Trail Loop", "AWU2-BLU-TRL", (decimal?)42500m, 9, "49mm", "Blue") }
            },
            new {
                Name = "Garmin Fenix 7 Pro Solar Edition",
                Desc = "Multisport GPS smartwatch with solar charging lens, built-in LED flashlight, and advanced training metrics.",
                Sku = "GARMIN-FENIX-7-PRO",
                Price = 38500.00m,
                CompareAt = 41900.00m,
                Stock = 14,
                CategorySlug = "smartwatches",
                BrandName = "Garmin",
                Image = "/images/seed/products/garmin-fenix-7.svg",
                Variants = new[] { ("47mm - Slate Gray", "GF7-47-SLT", (decimal?)38500m, 8, "47mm", "Slate Gray"), ("51mmX - Carbon Gray", "GF7X-51-CRB", (decimal?)42000m, 6, "51mm", "Carbon Gray") }
            },
            new {
                Name = "Canon EOS R6 Mark II Mirrorless Camera",
                Desc = "24.2 MP full-frame CMOS sensor, 4K 60p 10-bit video, Dual Pixel CMOS AF II, and in-body image stabilization.",
                Sku = "CANON-EOS-R6-MK2",
                Price = 115000.00m,
                CompareAt = 125000.00m,
                Stock = 8,
                CategorySlug = "digital-cameras",
                BrandName = "Canon",
                Image = "/images/seed/products/canon-eos-r6.svg",
                Variants = new[] { ("Body Only", "R6M2-BODY", (decimal?)115000m, 4, "Body Only", "Black"), ("Kit w/ 24-105mm Lens", "R6M2-KIT", (decimal?)138000m, 4, "24-105mm Kit", "Black") }
            },
            new {
                Name = "Philips Airfryer XXL Connected 7.2L",
                Desc = "Rapid Air technology with 16 cooking functions, NutriU app integration, and fat removal technology.",
                Sku = "PHILIPS-AIRFRYER-XXL",
                Price = 11999.00m,
                CompareAt = 13500.00m,
                Stock = 30,
                CategorySlug = "kitchen-dining",
                BrandName = "Philips",
                Image = "/images/seed/products/philips-airfryer-xxl.svg",
                Variants = new[] { ("Black/Copper Edition", "HD9285-BLK", (decimal?)11999m, 30, "7.2 Liters", "Black/Copper") }
            },
            new {
                Name = "Casio G-Shock Carbon Core Guard Watch",
                Desc = "Shock resistant, 200M water resistant, carbon core structure, dual time display with LED backlight.",
                Sku = "CASIO-GSHOCK-GA2100",
                Price = 6200.00m,
                CompareAt = 6999.00m,
                Stock = 40,
                CategorySlug = "smartwatches",
                BrandName = "Casio",
                Image = "/images/seed/products/casio-gshock.svg",
                Variants = new[] { ("Stealth All-Black", "GA2100-1A1", (decimal?)6200m, 25, "Standard", "Matte Black"), ("Military Green", "GA2100-1A3", (decimal?)6200m, 15, "Standard", "Olive Green") }
            },
            new {
                Name = "L'Oreal Paris Revitalift 1.5% Hyaluronic Acid Serum",
                Desc = "Intensive hydrating serum visibly plumps skin in 1 hour and reduces wrinkles with pure hyaluronic acid.",
                Sku = "LOREAL-REVITALIFT-HA",
                Price = 850.00m,
                CompareAt = 999.00m,
                Stock = 75,
                CategorySlug = "skincare",
                BrandName = "L'Oreal",
                Image = "/images/seed/products/loreal-serum.svg",
                Variants = new[] { ("30ml Bottle", "LRL-HA-30", (decimal?)850m, 45, "30ml", "Clear"), ("50ml Jumbo Bottle", "LRL-HA-50", (decimal?)1250m, 30, "50ml", "Clear") }
            },
            new {
                Name = "Adjustable Dumbbell Set 20KG with Connector",
                Desc = "Versatile 2-in-1 barbell dumbbell set with non-slip handles and durable rubber coated weight plates.",
                Sku = "FIT-DUMBBELL-20KG",
                Price = 3200.00m,
                CompareAt = 3800.00m,
                Stock = 30,
                CategorySlug = "fitness-gear",
                BrandName = "Nike",
                Image = "/images/seed/products/dumbbell-set-20kg.svg",
                Variants = new[] { ("20KG Pair Set", "DB-20KG", (decimal?)3200m, 20, "20KG", "Black/Red"), ("30KG Heavy Set", "DB-30KG", (decimal?)4400m, 10, "30KG", "Black/Red") }
            }
        };

        foreach (var spec in productSpecs)
        {
            var sku = spec.Sku.Trim();
            if (existingSkus.ContainsKey(sku) || await context.Products.AnyAsync(p => p.SKU == sku))
            {
                continue;
            }

            var p = Product.Create(
                spec.Name,
                spec.Desc,
                sku,
                spec.Price,
                spec.CompareAt,
                spec.Stock,
                GetCat(spec.CategorySlug),
                GetBrand(spec.BrandName),
                actorId);

            p.AddImage(spec.Image, actorId);

            foreach (var v in spec.Variants)
            {
                var vSku = v.Item2.Trim();
                if (!existingVariantSkus.Contains(vSku) && !await context.ProductVariants.AnyAsync(pv => pv.SKU == vSku))
                {
                    p.AddVariant(v.Item2, v.Item3, v.Item4, v.Item5, v.Item6, actorId);
                    existingVariantSkus.Add(vSku);
                }
            }

            try
            {
                context.Products.Add(p);
                await context.SaveChangesAsync();
                existing.Add(p);
                existingSkus[sku] = p;
            }
            catch (DbUpdateException ex)
            {
                logger.LogWarning("Skipping already existing product during seed: {Sku} ({Message})", sku, ex.Message);
                context.ChangeTracker.Clear();
            }
        }

        logger.LogInformation("Products seeding finished. Total: {Count}", existing.Count);
        return await context.Products.Include(p => p.Images).Include(p => p.Variants).ToListAsync();
    }
}
