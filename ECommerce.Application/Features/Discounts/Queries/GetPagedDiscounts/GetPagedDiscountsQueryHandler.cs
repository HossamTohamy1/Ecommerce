using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Queries.GetPagedDiscounts;

public class GetPagedDiscountsQueryHandler : IRequestHandler<GetPagedDiscountsQuery, PagedResult<DiscountDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPagedDiscountsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DiscountDto>> Handle(GetPagedDiscountsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.Set<Discount>().OrderByDescending(d => d.CreatedAt);
        var totalCount = await query.CountAsync(ct);

        var discounts = await query
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DiscountDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                DiscountType = d.DiscountType,
                Value = d.Value,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                MinimumOrderAmount = d.MinimumOrderAmount,
                UsageLimit = d.UsageLimit,
                UsageCount = d.UsageCount,
                IsActive = d.IsActive,
                ProductIds = d.ProductDiscounts.Select(pd => pd.ProductId).ToList()
            })
            .ToListAsync(ct);

        return new PagedResult<DiscountDto>
        {
            Items = discounts,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
