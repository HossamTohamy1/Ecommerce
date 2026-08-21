
namespace ECommerce.Infrastructure.Persistence.Configurations;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();

        builder.Property(w => w.UserId).IsRequired().HasMaxLength(450);

        builder.HasIndex(w => w.UserId).IsUnique();

        builder.HasMany(w => w.Items)
            .WithOne(i => i.Wishlist)
            .HasForeignKey(i => i.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
