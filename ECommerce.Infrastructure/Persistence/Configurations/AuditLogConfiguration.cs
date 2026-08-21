
namespace ECommerce.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UserId).HasMaxLength(450);
        builder.Property(a => a.UserName).HasMaxLength(150);
        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Changes).HasColumnType("nvarchar(max)");
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.IpAddress).HasMaxLength(64);

        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.UserId);
    }
}
