namespace ECommerce.Domain.Entities;

public enum NotificationType
{
    OrderCreated = 1,
    OrderStatusChanged = 2,
    NewReview = 3,
    ChatMessage = 4,
    General = 5
}

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string? Url { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
