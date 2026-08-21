
using ECommerce.Application.Features.Addresses.Commands.DeleteAddress;
using ECommerce.Application.Features.Addresses.Queries.GetMyAddresses;

namespace ECommerce.API.Pages.Addresses;

public class IndexModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    public List<AddressDto> Addresses { get; set; } = new();

    public async Task OnGetAsync()
    {
        Addresses = await _mediator.Send(new GetMyAddressesQuery(CurrentUserId));
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var result = await _mediator.Send(new DeleteAddressCommand(CurrentUserId, id));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage();
    }
}

