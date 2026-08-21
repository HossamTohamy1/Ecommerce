using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class ReviewSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        List<ApplicationUser> customers,
        List<Product> products,
        string actorId,
        ILogger logger)
    {
        var existingCount = await context.ProductReviews.CountAsync();
        if (existingCount >= 15)
        {
            return;
        }

        var reviewTexts = new (int Rating, string Comment)[]
        {
            (5, "Exceptional build quality and battery life! Exceeded all my expectations. Highly recommended!"),
            (5, "Original product with valid warranty. Fast shipping in Cairo, arrived within 24 hours."),
            (4, "Great performance and stunning design. Only minor drawback is the retail packaging was slightly dented."),
            (5, "منتج ممتاز جداً وجودة أصلية ١٠٠٪ والتوصيل كان سريع جداً في نفس اليوم."),
            (5, "Best purchase I've made this year. Premium feel, excellent display, and fast charging."),
            (4, "Very solid device for daily use. Great value for money compared to other alternatives."),
            (5, "خامات ممتازة وتغليف محترم جداً، شكراً لفريق لومير على الخدمة الاحترافية."),
            (5, "Crisp sound and superb active noise cancellation. Perfect for work and travel."),
            (4, "Comfortable fit and great style. True to size for footwear!"),
            (5, "The camera quality is breathtaking. Low light photos look like professional studio shots."),
            (5, "تجربة شراء رائعة والتعامل ممتاز وخدمة العملاء على الواتساب سريعة جداً."),
            (4, "Everything works as advertised. Very happy with the purchase."),
            (5, "Super lightweight and ultra fast performance. Runs heavy workloads effortlessly."),
            (5, "High quality material, very durable and looks even better in person."),
            (4, "Good product with nice packaging. Would definitely buy again from this store."),
            (5, "المنتج اصلي ومطابق للمواصفات تماما، خدمة ممتازة وتوصيل في الميعاد المحدد."),
            (5, "Outstanding customer support and seamless checkout experience with InstaPay!"),
            (4, "Very practical and user friendly. Has made my daily routine much easier.")
        };

        var reviews = new List<ProductReview>();
        for (int i = 0; i < reviewTexts.Length && i < products.Count; i++)
        {
            var customer = customers[i % customers.Count];
            var product = products[i];
            var (rating, comment) = reviewTexts[i];

            var review = ProductReview.Create(product.Id, customer.Id.ToString(), rating, comment, customer.Id.ToString());
            review.Approve();
            reviews.Add(review);
        }

        context.ProductReviews.AddRange(reviews);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} verified customer reviews", reviews.Count);
    }
}
