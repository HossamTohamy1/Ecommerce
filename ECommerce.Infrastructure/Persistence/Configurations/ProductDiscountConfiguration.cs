
namespace ECommerce.Infrastructure.Persistence.Configurations;

public class ProductDiscountConfiguration : IEntityTypeConfiguration<ProductDiscount>
{
    public void Configure(EntityTypeBuilder<ProductDiscount> builder)
    {
        builder.ToTable("ProductDiscounts");
        builder.HasKey(pd => pd.Id);
        builder.Property(pd => pd.Id).ValueGeneratedNever();

        builder.HasOne(pd => pd.Discount)
            .WithMany(d => d.ProductDiscounts)
            .HasForeignKey(pd => pd.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pd => new { pd.ProductId, pd.DiscountId }).IsUnique();

        builder.HasQueryFilter(pd => !pd.IsDeleted);
    }
}
