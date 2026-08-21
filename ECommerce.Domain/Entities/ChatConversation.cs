namespace ECommerce.Domain.Entities;

public class ChatConversation : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }

    public bool HasUnreadForAdmin { get; set; }
    public bool HasUnreadForCustomer { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
