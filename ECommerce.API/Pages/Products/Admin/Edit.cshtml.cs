using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Features.Brands.Queries.GetAllBrands;
using ECommerce.Application.Features.Categories.Queries.GetAllCategories;
using ECommerce.Application.Features.Products.Commands.DeleteProduct;
using ECommerce.Application.Features.Products.Commands.DeleteProductImage;
using ECommerce.Application.Features.Products.Commands.UpdateProduct;
using ECommerce.Application.Features.Products.Commands.UploadProductImages;
using ECommerce.Application.Features.Products.Queries.GetProductById;
using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Pages.Products.Admin;

public class EditModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public EditModel(
        IMediator mediator,
        IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public ProductDto? Product { get; set; }
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

        public bool IsActive { get; set; } = true;
    }

    private async Task LoadReferenceDataAsync()
    {
        Categories = await _mediator.Send(new GetAllCategoriesQuery());
        Brands = await _mediator.Send(new GetAllBrandsQuery());
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _mediator.Send(new GetProductByIdQuery(Id));
        if (!result.Succeeded || result.Data is null)
        {
            return RedirectToPage("/Products/Index");
        }

        Product = result.Data;
        Input = new InputModel
        {
            Name = Product.Name,
            Description = Product.Description,
            SKU = Product.SKU,
            Price = Product.Price,
            CompareAtPrice = Product.CompareAtPrice,
            StockQuantity = Product.StockQuantity,
            CategoryId = Product.CategoryId,
            BrandId = Product.BrandId,
            IsActive = Product.IsActive
        };

        await LoadReferenceDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateDetailsAsync()
    {
        await LoadReferenceDataAsync();
        Product = (await _mediator.Send(new GetProductByIdQuery(Id))).Data;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _mediator.Send(new UpdateProductCommand(Id, new UpdateProductRequest
        {
            Name = Input.Name,
            Description = Input.Description,
            SKU = Input.SKU,
            Price = Input.Price,
            CompareAtPrice = Input.CompareAtPrice,
            StockQuantity = Input.StockQuantity,
            CategoryId = Input.CategoryId,
            BrandId = Input.BrandId,
            IsActive = Input.IsActive
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

    public async Task<IActionResult> OnPostUploadImageAsync([FromForm] IFormFile? image, [FromForm] List<IFormFile>? images)
    {
        var file = image ?? images?.FirstOrDefault();
        if (file is not null)
        {
            var result = await _mediator.Send(new UploadProductImagesCommand(Id, new List<IFormFile> { file }, CurrentUserId));
            if (result.Succeeded)
            {
                SetSuccess(_localizer);
            }
            else
            {
                SetError(result.Error);
            }
        }
        else
        {
            SetError(_localizer["Files.AtLeastOneRequired"].Value);
        }

        return RedirectToPage(new { id = Id });
    }

    public Task<IActionResult> OnPostUploadImagesAsync([FromForm] IFormFile? image, [FromForm] List<IFormFile>? images)
        => OnPostUploadImageAsync(image, images);

    public async Task<IActionResult> OnPostDeleteImageAsync(Guid? imageId)
    {
        var productResult = await _mediator.Send(new GetProductByIdQuery(Id));
        var targetImageId = imageId ?? productResult.Data?.Images.FirstOrDefault()?.Id;

        if (targetImageId.HasValue && targetImageId.Value != Guid.Empty)
        {
            var result = await _mediator.Send(new DeleteProductImageCommand(Id, targetImageId.Value));
            if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        }

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (!User.IsInRole(AppConstants.Roles.Admin))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new DeleteProductCommand(Id));
        if (result.Succeeded)
        {
            SetSuccess(_localizer);
            return RedirectToPage("/Products/Index");
        }

        SetError(result.Error);
        return RedirectToPage(new { id = Id });
    }
}
