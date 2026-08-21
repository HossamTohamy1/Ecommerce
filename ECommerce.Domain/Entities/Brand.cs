namespace ECommerce.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; }

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Brand()
    {
    }

    public static Brand Create(string name, string actorId)
    {
        var brand = new Brand { CreatedById = actorId };
        brand.Rename(name);
        return brand;
    }

    public void UpdateDetails(string name, bool isActive, string actorId)
    {
        Rename(name);
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = actorId;
    }

    public void SetLogo(string logoUrl, string actorId)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
        {
            throw new DomainException("Brand.MissingLogoUrl", "A logo must have a URL.");
        }

        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = actorId;
    }

    private void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Brand.NameRequired", "A brand must have a name.");
        }

        Name = name.Trim();
    }
}
