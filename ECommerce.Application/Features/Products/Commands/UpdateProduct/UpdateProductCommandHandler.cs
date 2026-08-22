using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateProductCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (product is null)
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        var normalizedSku = command.Request.SKU?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedSku))
        {
            return Result<ProductDto>.Failure(_localizer["Validation.Required"].Value);
        }

        if (await _context.Set<Product>().AnyAsync(p => p.SKU.ToLower() == normalizedSku.ToLower() && p.Id != command.Id, ct))
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Product.DuplicateSku"].Value);
        }

        var categoryName = await _context.Set<Category>()
            .Where(c => c.Id == command.Request.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);

        if (categoryName is null)
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Category.NotFound"].Value);
        }

        string? brandName = null;
        if (command.Request.BrandId.HasValue)
        {
            brandName = await _context.Set<Brand>()
                .Where(b => b.Id == command.Request.BrandId.Value)
                .Select(b => b.Name)
                .FirstOrDefaultAsync(ct);

            if (brandName is null)
            {
                return Result<ProductDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value);
            }
        }

        try
        {
            product.UpdateDetails(
                command.Request.Name?.Trim() ?? string.Empty,
                command.Request.Description,
                normalizedSku,
                command.Request.Price,
                command.Request.CompareAtPrice,
                command.Request.StockQuantity,
                command.Request.CategoryId,
                command.Request.BrandId,
                command.Request.IsActive,
                command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<ProductDto>.Failure(LocalizeDomainError(ex));
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Product.DuplicateSku"].Value);
        }

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            BrandId = product.BrandId,
            BrandName = brandName,
            Images = product.Images
                .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain,
                    DisplayOrder = i.DisplayOrder
                })
                .ToList(),
            Variants = product.Variants
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    SKU = v.SKU,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Size = v.Size,
                    Color = v.Color
                })
                .ToList()
        };

        return Result<ProductDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Catalog.ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "Variant.NotFound" => _localizer["Catalog.Variant.NotFound"].Value,
        _ => ex.Message
    };
}
