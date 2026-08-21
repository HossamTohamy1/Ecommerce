using ECommerce.Application.DTOs.Discounts;
using ECommerce.Application.Features.Discounts.Commands.DeleteDiscount;
using ECommerce.Application.Features.Discounts.Queries.GetPagedDiscounts;

namespace ECommerce.API.Pages.Discounts;

public class IndexModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    public PagedResult<DiscountDto> Result { get; set; } = new();

    public async Task OnGetAsync([FromQuery] int? page, [FromQuery] int? pageNumber)
    {
        var p = (page.HasValue && page.Value > 0) ? page.Value :
                ((pageNumber.HasValue && pageNumber.Value > 0) ? pageNumber.Value :
                (PageNumber > 0 ? PageNumber : 1));

        PageNumber = p;
        Result = await _mediator.Send(new GetPagedDiscountsQuery(p, 10));
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!User.IsInRole(AppConstants.Roles.Admin))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new DeleteDiscountCommand(id));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage();
    }
}

