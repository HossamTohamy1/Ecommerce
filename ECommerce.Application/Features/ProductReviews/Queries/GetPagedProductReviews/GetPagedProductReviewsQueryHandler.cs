using ECommerce.Application.DTOs.Reviews;

namespace ECommerce.Application.Features.ProductReviews.Queries.GetPagedProductReviews;

public class GetPagedProductReviewsQueryHandler : IRequestHandler<GetPagedProductReviewsQuery, PagedResult<ProductReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPagedProductReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductReviewDto>> Handle(GetPagedProductReviewsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.Set<ProductReview>().AsNoTracking().OrderByDescending(r => r.CreatedAt);
        var totalCount = await query.CountAsync(ct);

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userGuids = reviews
            .Select(r => r.UserId)
            .Distinct()
            .Where(id => Guid.TryParse(id, out _))
            .Select(Guid.Parse)
            .ToList();

        var names = await _context.Set<ApplicationUser>()
            .Where(u => userGuids.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id.ToString(), u => u.FullName, ct);

        var mapped = reviews.Select(r => new ProductReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserId = r.UserId,
            UserFullName = names.GetValueOrDefault(r.UserId, "N/A"),
            Rating = r.Rating,
            Comment = r.Comment,
            IsApproved = r.IsApproved,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedResult<ProductReviewDto>
        {
            Items = mapped,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
