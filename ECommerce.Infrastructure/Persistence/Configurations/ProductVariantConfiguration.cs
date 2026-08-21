
namespace ECommerce.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.SKU).IsRequired().HasMaxLength(50);
        builder.Property(v => v.Price).HasColumnType("decimal(18,2)");
        builder.Property(v => v.Size).HasMaxLength(50);
        builder.Property(v => v.Color).HasMaxLength(50);

        builder.HasIndex(v => v.SKU).IsUnique();

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
