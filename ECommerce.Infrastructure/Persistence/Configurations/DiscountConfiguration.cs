
namespace ECommerce.Infrastructure.Persistence.Configurations;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Code).HasMaxLength(50);
        builder.Property(d => d.Value).HasColumnType("decimal(18,2)");
        builder.Property(d => d.MinimumOrderAmount).HasColumnType("decimal(18,2)");
        builder.Property(d => d.DiscountType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(d => d.Code).IsUnique().HasFilter("[Code] IS NOT NULL");

        builder.Navigation(d => d.ProductDiscounts).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
