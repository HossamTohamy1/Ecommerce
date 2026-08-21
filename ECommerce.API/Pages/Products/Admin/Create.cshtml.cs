using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Features.Brands.Queries.GetAllBrands;
using ECommerce.Application.Features.Categories.Queries.GetAllCategories;
using ECommerce.Application.Features.Products.Commands.CreateProduct;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Pages.Products.Admin;

public class CreateModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateModel(
        IMediator mediator,
        IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<CategoryDto> Categories { get; set; } = new();
    public List<BrandDto> Brands { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public decimal? CompareAtPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public IFormFile? Image { get; set; }
    }

    public async Task OnGetAsync()
    {
        Categories = await _mediator.Send(new GetAllCategoriesQuery());
        Brands = await _mediator.Send(new GetAllBrandsQuery());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await _mediator.Send(new GetAllCategoriesQuery());
        Brands = await _mediator.Send(new GetAllBrandsQuery());

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var imageList = Input.Image != null ? new List<IFormFile> { Input.Image } : null;

        var result = await _mediator.Send(new CreateProductCommand(new CreateProductRequest
        {
            Name = Input.Name,
            Description = Input.Description,
            SKU = Input.SKU,
            Price = Input.Price,
            CompareAtPrice = Input.CompareAtPrice,
            StockQuantity = Input.StockQuantity,
            CategoryId = Input.CategoryId,
            BrandId = Input.BrandId,
            Images = imageList
        }, CurrentUserId));

        if (!result.Succeeded)
        {
            if (result.Error?.Contains("SKU", StringComparison.OrdinalIgnoreCase) == true)
            {
                ModelState.AddModelError("Input.SKU", result.Error);
            }
            else
            {
                ModelState.AddModelError("Input.Name", result.Error ?? "Error");
            }
            SetError(result.Error);
            return Page();
        }

        SetSuccess(_localizer);
        return RedirectToPage("/Products/Index");
    }
}

