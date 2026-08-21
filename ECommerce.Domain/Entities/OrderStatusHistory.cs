namespace ECommerce.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public OrderStatus Status { get; private set; }
    public string? Note { get; private set; }

    private OrderStatusHistory()
    {
    }

    internal static OrderStatusHistory Create(Guid orderId, OrderStatus status, string? note, string actorId)
    {
        return new OrderStatusHistory
        {
            OrderId = orderId,
            Status = status,
            Note = note,
            CreatedById = actorId
        };
    }
}
