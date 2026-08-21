using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Features.ProductReviews.Commands.ApproveProductReview;
using ECommerce.Application.Features.ProductReviews.Commands.DeleteProductReviewAsAdmin;
using ECommerce.Application.Features.ProductReviews.Queries.GetPagedProductReviews;

namespace ECommerce.API.Pages.Reviews;

public class ModerationModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ModerationModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    public PagedResult<ProductReviewDto> Result { get; set; } = new();

    public async Task OnGetAsync([FromQuery] int? page, [FromQuery] int? pageNumber)
    {
        var p = (page.HasValue && page.Value > 0) ? page.Value :
                ((pageNumber.HasValue && pageNumber.Value > 0) ? pageNumber.Value :
                (PageNumber > 0 ? PageNumber : 1));

        PageNumber = p;
        Result = await _mediator.Send(new GetPagedProductReviewsQuery(p, 10));
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        if (!User.IsInRole(AppConstants.Roles.Admin))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new ApproveProductReviewCommand(id));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!User.IsInRole(AppConstants.Roles.Admin))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new DeleteProductReviewAsAdminCommand(id));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage();
    }
}

