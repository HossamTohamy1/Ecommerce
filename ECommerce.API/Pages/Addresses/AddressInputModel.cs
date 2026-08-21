
namespace ECommerce.API.Pages.Addresses;

public class AddressInputModel
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(250, MinimumLength = 5)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Governorate { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PostalCode { get; set; }

    public bool IsDefault { get; set; }
}
