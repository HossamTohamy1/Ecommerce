
namespace ECommerce.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.OrderNumber)
            .HasConversion(n => n.Value, v => OrderNumber.Of(v))
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.UserId).IsRequired().HasMaxLength(450);

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);

        builder.Property(o => o.SubTotal).HasMoneyConversion().HasColumnType("decimal(18,2)");
        builder.Property(o => o.DiscountAmount).HasMoneyConversion().HasColumnType("decimal(18,2)");
        builder.Property(o => o.ShippingFee).HasMoneyConversion().HasColumnType("decimal(18,2)");
        builder.Property(o => o.TotalAmount).HasMoneyConversion().HasColumnType("decimal(18,2)");

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.UserId);

        builder.HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
