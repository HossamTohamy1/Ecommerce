
using ECommerce.Application.Features.Addresses.Commands.UpdateAddress;
using ECommerce.Application.Features.Addresses.Queries.GetMyAddresses;

namespace ECommerce.API.Pages.Addresses;

public class EditModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public EditModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public AddressInputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var addresses = await _mediator.Send(new GetMyAddressesQuery(CurrentUserId));
        var address = addresses.FirstOrDefault(a => a.Id == Id);
        if (address is null)
        {
            return RedirectToPage("/Addresses/Index");
        }

        Input = new AddressInputModel
        {
            FullName = address.FullName,
            Phone = address.Phone,
            Street = address.Street,
            City = address.City,
            Governorate = address.Governorate,
            PostalCode = address.PostalCode,
            IsDefault = address.IsDefault
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new UpdateAddressCommand(CurrentUserId, Id, new SaveAddressRequest
        {
            FullName = Input.FullName,
            Phone = Input.Phone,
            Street = Input.Street,
            City = Input.City,
            Governorate = Input.Governorate,
            PostalCode = Input.PostalCode,
            IsDefault = Input.IsDefault
        }));

        if (!result.Succeeded)
        {
            SetError(result.Error);
            return Page();
        }

        SetSuccess(_localizer);

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }

        return RedirectToPage("/Addresses/Index");
    }
}

