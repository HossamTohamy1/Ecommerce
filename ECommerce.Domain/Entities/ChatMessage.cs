namespace ECommerce.Domain.Entities;

public enum ChatSenderRole
{
    Customer = 1,
    Admin = 2
}

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public ChatConversation Conversation { get; set; } = null!;

    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public ChatSenderRole SenderRole { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }
}
