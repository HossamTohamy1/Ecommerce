using ECommerce.Application.Features.Brands.Queries.GetAllBrands;
using ECommerce.Application.Features.Categories.Queries.GetAllCategories;
using ECommerce.Application.Features.Products.Commands.DeleteProduct;
using ECommerce.Application.Features.Products.Queries.GetProducts;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace ECommerce.API.Pages.Products;

public class IndexModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? BrandId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public PagedResult<ProductDto> Result { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<BrandDto> Brands { get; set; } = new();

    public async Task OnGetAsync([FromQuery] int? page, [FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        var p = (page.HasValue && page.Value > 0) ? page.Value :
                ((pageNumber.HasValue && pageNumber.Value > 0) ? pageNumber.Value :
                (PageNumber > 0 ? PageNumber : 1));

        var ps = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value :
                 (PageSize is >= 1 and <= 100 ? PageSize : 10);

        PageNumber = p;
        PageSize = ps;

        Categories = await _mediator.Send(new GetAllCategoriesQuery());
        Brands = await _mediator.Send(new GetAllBrandsQuery());

        Result = await _mediator.Send(new GetProductsQuery(new ProductListQuery
        {
            CategoryId = CategoryId,
            BrandId = BrandId,
            Search = Search,
            Page = p,
            PageSize = ps
        }));
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!User.IsInRole(AppConstants.Roles.Admin))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new DeleteProductCommand(id));
        if (result.Succeeded)
        {
            SetSuccess(_localizer);
        }
        else
        {
            SetError(result.Error);
        }

        return RedirectToPage("/Products/Index", new { page = PageNumber, search = Search, categoryId = CategoryId, brandId = BrandId, pageSize = PageSize });
    }
}
