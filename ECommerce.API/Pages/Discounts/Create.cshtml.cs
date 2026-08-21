using ECommerce.Application.DTOs.Discounts;
using ECommerce.Application.Features.Discounts.Commands.CreateDiscount;
using ECommerce.Domain.Entities;

namespace ECommerce.API.Pages.Discounts;

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
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public List<Guid> SelectedProductIds { get; set; } = new();

    public List<ECommerce.Application.DTOs.Catalog.ProductDto> AllProducts { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Code { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        [Range(0.01, double.MaxValue)]
        public decimal Value { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddMonths(1);

        public decimal? MinimumOrderAmount { get; set; }
        public int? UsageLimit { get; set; }
    }

    private async Task LoadProductsAsync()
    {
        var result = await _mediator.Send(new ECommerce.Application.Features.Products.Queries.GetProducts.GetProductsQuery(
            new ECommerce.Application.DTOs.Catalog.ProductListQuery { PageSize = 500 }));
        AllProducts = result.Items.DistinctBy(p => p.Id).ToList();
    }

    public async Task OnGetAsync()
    {
        await LoadProductsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadProductsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new CreateDiscountCommand(new CreateDiscountRequest
        {
            Name = Input.Name,
            Code = Input.Code,
            DiscountType = Input.DiscountType,
            Value = Input.Value,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            MinimumOrderAmount = Input.MinimumOrderAmount,
            UsageLimit = Input.UsageLimit,
            ProductIds = SelectedProductIds
        }, CurrentUserId));

        if (!result.Succeeded)
        {
            if (!string.IsNullOrEmpty(Input.Code) && result.Error?.Contains("Code", StringComparison.OrdinalIgnoreCase) == true)
            {
                ModelState.AddModelError("Input.Code", result.Error);
            }
            else
            {
                ModelState.AddModelError("Input.Name", result.Error ?? "Error");
            }
            SetError(result.Error);
            return Page();
        }

        SetSuccess(_localizer);
        return RedirectToPage("/Discounts/Index");
    }
}

