using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Queries.GetAllDiscounts;

public class GetAllDiscountsQueryHandler : IRequestHandler<GetAllDiscountsQuery, List<DiscountDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllDiscountsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DiscountDto>> Handle(GetAllDiscountsQuery request, CancellationToken ct)
    {
        return await _context.Set<Discount>()
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
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
    }
}
