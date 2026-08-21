using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDiscountResolver _discountResolver;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetProductByIdQueryHandler(IApplicationDbContext context, IDiscountResolver discountResolver, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _discountResolver = discountResolver;
        _localizer = localizer;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var dto = await _context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
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

        if (dto is null)
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        var activeDiscount = await _discountResolver.GetActiveDiscountForProductAsync(dto.Id, DateTime.UtcNow, ct);
        if (activeDiscount is not null)
        {
            dto.DiscountName = activeDiscount.Name;
            dto.DiscountType = activeDiscount.DiscountType;
            dto.DiscountValue = activeDiscount.Value;
            dto.DiscountAmount = _discountResolver.CalculateDiscountAmount(dto.Price, activeDiscount);
            dto.DiscountedPrice = _discountResolver.CalculateDiscountedPrice(dto.Price, activeDiscount);
        }

        return Result<ProductDto>.Success(dto);
    }
}
