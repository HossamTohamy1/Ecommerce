
namespace ECommerce.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.SenderId).IsRequired().HasMaxLength(450);
        builder.Property(m => m.SenderName).IsRequired().HasMaxLength(150);
        builder.Property(m => m.SenderRole).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Content).IsRequired().HasMaxLength(2000);

        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.CreatedAt);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
