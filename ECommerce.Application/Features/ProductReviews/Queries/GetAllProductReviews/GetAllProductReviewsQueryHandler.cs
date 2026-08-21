using ECommerce.Application.DTOs.Reviews;

namespace ECommerce.Application.Features.ProductReviews.Queries.GetAllProductReviews;

public class GetAllProductReviewsQueryHandler : IRequestHandler<GetAllProductReviewsQuery, List<ProductReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllProductReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductReviewDto>> Handle(GetAllProductReviewsQuery request, CancellationToken ct)
    {
        var reviews = await _context.Set<ProductReview>()
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
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

        return reviews.Select(r => new ProductReviewDto
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
    }
}
