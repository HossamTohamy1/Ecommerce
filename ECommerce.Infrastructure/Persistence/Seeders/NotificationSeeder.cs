using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class NotificationSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        ApplicationUser admin,
        List<ApplicationUser> customers,
        string actorId,
        ILogger logger)
    {
        var existingCount = await context.Notifications.CountAsync();
        if (existingCount >= 15)
        {
            return;
        }

        var notifications = new List<Notification>
        {
            new()
            {
                UserId = admin.Id.ToString(),
                Type = NotificationType.OrderCreated,
                Title = "New Order #ORD-10001",
                Message = "Customer Demo Customer placed a new order for 64,999.00 EGP",
                Url = "/Orders/Admin/AllOrders",
                IsRead = false,
                CreatedById = actorId
            },
            new()
            {
                UserId = admin.Id.ToString(),
                Type = NotificationType.NewReview,
                Title = "New Product Review Submitted",
                Message = "A 5-star review was submitted for iPhone 16 Pro Max",
                Url = "/Reviews/Moderation",
                IsRead = false,
                CreatedById = actorId
            },
            new()
            {
                UserId = admin.Id.ToString(),
                Type = NotificationType.General,
                Title = "Daily Catalog Inventory Report",
                Message = "18 products in stock, 0 out-of-stock items detected.",
                Url = "/Products/Index",
                IsRead = true,
                ReadAt = DateTime.UtcNow.AddHours(-2),
                CreatedById = actorId
            }
        };

        for (int i = 0; i < 15 && i < customers.Count; i++)
        {
            var customer = customers[i];
            notifications.Add(new Notification
            {
                UserId = customer.Id.ToString(),
                Type = NotificationType.OrderStatusChanged,
                Title = "Order Confirmed & Shipping Soon",
                Message = $"Your Lumaire order has been confirmed and is being prepared for dispatch.",
                Url = "/Orders/Index",
                IsRead = (i % 2 == 0),
                ReadAt = (i % 2 == 0) ? DateTime.UtcNow.AddDays(-1) : null,
                CreatedById = actorId
            });
        }

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} system notifications", notifications.Count);
    }
}
