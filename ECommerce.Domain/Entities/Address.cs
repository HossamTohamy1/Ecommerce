namespace ECommerce.Domain.Entities;

public class Address : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Governorate { get; private set; } = string.Empty;
    public string? PostalCode { get; private set; }
    public bool IsDefault { get; private set; }

    private Address()
    {
    }

    public static Address Create(string userId, string fullName, string phone, string street, string city, string governorate, string? postalCode, bool isDefault, string actorId)
    {
        ValidateDetails(fullName, phone, street, city, governorate);

        return new Address
        {
            UserId = userId,
            FullName = fullName.Trim(),
            Phone = phone.Trim(),
            Street = street.Trim(),
            City = city.Trim(),
            Governorate = governorate.Trim(),
            PostalCode = postalCode,
            IsDefault = isDefault,
            CreatedById = actorId
        };
    }

    public void UpdateDetails(string fullName, string phone, string street, string city, string governorate, string? postalCode, bool isDefault, string actorId)
    {
        ValidateDetails(fullName, phone, street, city, governorate);

        FullName = fullName.Trim();
        Phone = phone.Trim();
        Street = street.Trim();
        City = city.Trim();
        Governorate = governorate.Trim();
        PostalCode = postalCode;
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = actorId;
    }

    public void UnsetDefault() => IsDefault = false;

    private static void ValidateDetails(string fullName, string phone, string street, string city, string governorate)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(street)
            || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(governorate))
        {
            throw new DomainException("Address.RequiredFieldsMissing", "Full name, phone, street, city and governorate are required.");
        }
    }
}
