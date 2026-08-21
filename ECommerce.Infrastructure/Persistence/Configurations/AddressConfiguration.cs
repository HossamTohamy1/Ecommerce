
namespace ECommerce.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired().HasMaxLength(450);
        builder.Property(a => a.FullName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Phone).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Street).IsRequired().HasMaxLength(250);
        builder.Property(a => a.City).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Governorate).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(20);

        builder.HasIndex(a => a.UserId);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
