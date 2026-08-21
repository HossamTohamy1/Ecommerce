using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence.Seeders;

public static class OrderSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        List<ApplicationUser> customers,
        List<Product> products,
        string actorId,
        ILogger logger)
    {
        var existingCount = await context.Orders.CountAsync();
        if (existingCount >= 15)
        {
            return;
        }

        var addresses = await context.Addresses.ToListAsync();
        if (addresses.Count == 0 || customers.Count == 0 || products.Count == 0)
        {
            return;
        }

        var orderStatuses = new[]
        {
            OrderStatus.Delivered,
            OrderStatus.Shipped,
            OrderStatus.Confirmed,
            OrderStatus.Processing,
            OrderStatus.Pending,
            OrderStatus.Delivered,
            OrderStatus.Delivered,
            OrderStatus.Shipped,
            OrderStatus.Confirmed,
            OrderStatus.Delivered,
            OrderStatus.Processing,
            OrderStatus.Delivered,
            OrderStatus.Cancelled,
            OrderStatus.Delivered,
            OrderStatus.Shipped,
            OrderStatus.Confirmed
        };

        var orders = new List<Order>();
        for (int i = 0; i < orderStatuses.Length; i++)
        {
            var customer = customers[i % customers.Count];
            var address = addresses.FirstOrDefault(a => a.UserId == customer.Id.ToString()) ?? addresses[0];
            var paymentMethod = (i % 2 == 0) ? PaymentMethod.Cash : PaymentMethod.BankTransfer;
            var shippingFee = Money.Of(50m);

            var order = Order.Create(customer.Id.ToString(), address.Id, paymentMethod, shippingFee, customer.Id.ToString());

            var p1 = products[i % products.Count];
            order.AddItem(p1.Id, null, p1.Name, 1, Money.Of(p1.Price), Money.Zero, customer.Id.ToString());

            if (i % 3 == 0)
            {
                var p2 = products[(i + 1) % products.Count];
                order.AddItem(p2.Id, null, p2.Name, 1, Money.Of(p2.Price), Money.Zero, customer.Id.ToString());
            }

            var targetStatus = orderStatuses[i];
            if (targetStatus != OrderStatus.Pending)
            {
                if (targetStatus == OrderStatus.Cancelled)
                {
                    order.ChangeStatus(OrderStatus.Cancelled, "Customer requested cancellation before shipment.", actorId);
                }
                else
                {
                    order.ChangeStatus(OrderStatus.Confirmed, "Order verified and inventory reserved.", actorId);
                    if (targetStatus >= OrderStatus.Processing)
                    {
                        order.ChangeStatus(OrderStatus.Processing, "Order is being packed at logistics center.", actorId);
                    }
                    if (targetStatus >= OrderStatus.Shipped)
                    {
                        order.ChangeStatus(OrderStatus.Shipped, "Package handed to courier (Tracking #EG" + Random.Shared.Next(100000, 999999) + ").", actorId);
                    }
                    if (targetStatus == OrderStatus.Delivered)
                    {
                        order.ChangeStatus(OrderStatus.Delivered, "Delivered and signed by recipient.", actorId);
                    }
                }
            }

            orders.Add(order);
        }

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} orders", orders.Count);
    }
}
