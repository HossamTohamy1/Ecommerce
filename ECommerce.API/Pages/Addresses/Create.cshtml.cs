
using ECommerce.Application.Features.Addresses.Commands.CreateAddress;

namespace ECommerce.API.Pages.Addresses;

public class CreateModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty]
    public AddressInputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new CreateAddressCommand(CurrentUserId, new SaveAddressRequest
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

