using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Features.Orders.Queries.GetAllOrdersPaged;

namespace ECommerce.API.Pages.Orders.Admin;

public class AllOrdersModel : RazorPageBase
{
    private readonly IMediator _mediator;

    public AllOrdersModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    public PagedResult<OrderDto> Result { get; set; } = new();

    public async Task OnGetAsync([FromQuery] int? page, [FromQuery] int? pageNumber)
    {
        var p = (page.HasValue && page.Value > 0) ? page.Value :
                ((pageNumber.HasValue && pageNumber.Value > 0) ? pageNumber.Value :
                (PageNumber > 0 ? PageNumber : 1));

        PageNumber = p;
        Result = await _mediator.Send(new GetAllOrdersPagedQuery(p, 10));
    }
}

