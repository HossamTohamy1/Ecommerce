using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Queries.GetDiscountById;

public class GetDiscountByIdQueryHandler : IRequestHandler<GetDiscountByIdQuery, Result<DiscountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetDiscountByIdQueryHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<DiscountDto>> Handle(GetDiscountByIdQuery request, CancellationToken ct)
    {
        var discount = await _context.Set<Discount>()
            .AsNoTracking()
            .Where(d => d.Id == request.Id)
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
            .FirstOrDefaultAsync(ct);

        return discount is null ? Result<DiscountDto>.Failure(_localizer["Discount.NotFound"].Value) : Result<DiscountDto>.Success(discount);
    }
}
