using ECommerce.Application.DTOs.Discounts;
using ECommerce.Application.Features.Discounts.Commands.AssignProductToDiscount;
using ECommerce.Application.Features.Discounts.Commands.RemoveProductFromDiscount;
using ECommerce.Application.Features.Discounts.Commands.UpdateDiscount;
using ECommerce.Application.Features.Discounts.Queries.GetDiscountById;
using ECommerce.Application.Features.Products.Queries.GetProducts;
using ECommerce.Domain.Entities;

namespace ECommerce.API.Pages.Discounts;

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
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public Guid AssignProductId { get; set; }

    public DiscountDto? Discount { get; set; }
    public List<ProductDto> AssignedProducts { get; set; } = new();
    public List<ProductDto> AllProducts { get; set; } = new();
    public List<ProductDto> AvailableProducts { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Code { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Value { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public decimal? MinimumOrderAmount { get; set; }
        public int? UsageLimit { get; set; }
        public bool IsActive { get; set; } = true;
    }

    private async Task LoadAsync()
    {
        var result = await _mediator.Send(new GetDiscountByIdQuery(Id));
        Discount = result.Data;

        var allProducts = await _mediator.Send(new GetProductsQuery(new ProductListQuery { PageSize = 500 }));
        AllProducts = allProducts.Items.DistinctBy(p => p.Id).ToList();
        AssignedProducts = Discount is null
            ? new List<ProductDto>()
            : AllProducts.Where(p => Discount.ProductIds.Contains(p.Id)).ToList();
        AvailableProducts = AllProducts.Where(p => !(Discount?.ProductIds.Contains(p.Id) ?? false)).ToList();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAsync();
        if (Discount is null)
        {
            return RedirectToPage("/Discounts/Index");
        }

        Input = new InputModel
        {
            Name = Discount.Name,
            Code = Discount.Code,
            DiscountType = Discount.DiscountType,
            Value = Discount.Value,
            StartDate = Discount.StartDate,
            EndDate = Discount.EndDate,
            MinimumOrderAmount = Discount.MinimumOrderAmount,
            UsageLimit = Discount.UsageLimit,
            IsActive = Discount.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        await LoadAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new UpdateDiscountCommand(Id, new UpdateDiscountRequest
        {
            Name = Input.Name,
            Code = Input.Code,
            DiscountType = Input.DiscountType,
            Value = Input.Value,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            MinimumOrderAmount = Input.MinimumOrderAmount,
            UsageLimit = Input.UsageLimit,
            IsActive = Input.IsActive
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

    public async Task<IActionResult> OnPostAssignProductAsync()
    {
        var result = await _mediator.Send(new AssignProductToDiscountCommand(Id, AssignProductId, CurrentUserId));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostRemoveProductAsync(Guid productId)
    {
        var result = await _mediator.Send(new RemoveProductFromDiscountCommand(Id, productId));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage(new { id = Id });
    }
}

