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
        var product = await _context.Set<Product>().FirstOrDefaultAsync(p => p.Id == command.Id, ct);
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

        if (!await _context.Set<Category>().AnyAsync(c => c.Id == command.Request.CategoryId, ct))
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Category.NotFound"].Value);
        }

        if (command.Request.BrandId.HasValue && !await _context.Set<Brand>().AnyAsync(b => b.Id == command.Request.BrandId, ct))
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value);
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

        var dto = await _context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Id == command.Id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SKU = p.SKU,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                Images = p.Images
                    .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageDto { Id = i.Id, ImageUrl = i.ImageUrl, IsMain = i.IsMain, DisplayOrder = i.DisplayOrder })
                    .ToList(),
                Variants = p.Variants
                    .Select(v => new ProductVariantDto { Id = v.Id, SKU = v.SKU, Price = v.Price, StockQuantity = v.StockQuantity, Size = v.Size, Color = v.Color })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<ProductDto>.Failure(_localizer["Catalog.Product.NotFound"].Value) : Result<ProductDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Catalog.ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "Variant.NotFound" => _localizer["Catalog.Variant.NotFound"].Value,
        _ => ex.Message
    };
}
