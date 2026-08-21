namespace ECommerce.Infrastructure.Persistence.Configurations;

public class TwoFactorCodeConfiguration : IEntityTypeConfiguration<TwoFactorCode>
{
    public void Configure(EntityTypeBuilder<TwoFactorCode> builder)
    {
        builder.ToTable("TwoFactorCodes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.CodeHash).IsRequired().HasMaxLength(64);
        builder.HasIndex(t => new { t.UserId, t.ExpiresAtUtc });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
