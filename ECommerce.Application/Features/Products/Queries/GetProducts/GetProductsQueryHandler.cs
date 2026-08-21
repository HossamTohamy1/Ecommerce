using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDiscountResolver _discountResolver;

    public GetProductsQueryHandler(IApplicationDbContext context, IDiscountResolver discountResolver)
    {
        _context = context;
        _discountResolver = discountResolver;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var page = request.Query.Page < 1 ? 1 : request.Query.Page;
        var pageSize = request.Query.PageSize is < 1 or > 100 ? 20 : request.Query.PageSize;

        var baseQuery = _context.Set<Product>().AsQueryable();

        if (request.Query.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.CategoryId == request.Query.CategoryId);
        }

        if (request.Query.BrandId.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.BrandId == request.Query.BrandId);
        }

        if (!string.IsNullOrWhiteSpace(request.Query.Search))
        {
            var term = request.Query.Search.Trim();
            baseQuery = baseQuery.Where(p => p.Name.Contains(term) || p.SKU.Contains(term));
        }

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .ToListAsync(ct);

        if (items.Count > 0)
        {
            var now = DateTime.UtcNow;
            var productIds = items.Select(p => p.Id).ToList();
            var activeDiscounts = await _discountResolver.GetActiveDiscountsForProductsAsync(productIds, now, ct);

            foreach (var item in items)
            {
                if (activeDiscounts.TryGetValue(item.Id, out var discount))
                {
                    item.DiscountName = discount.Name;
                    item.DiscountType = discount.DiscountType;
                    item.DiscountValue = discount.Value;
                    item.DiscountAmount = _discountResolver.CalculateDiscountAmount(item.Price, discount);
                    item.DiscountedPrice = _discountResolver.CalculateDiscountedPrice(item.Price, discount);
                }
            }
        }

        return new PagedResult<ProductDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

