using ECommerce.Application.DTOs.Reviews;

namespace ECommerce.Application.Features.ProductReviews.Commands.UpdateProductReview;

public class UpdateProductReviewCommandHandler : IRequestHandler<UpdateProductReviewCommand, Result<ProductReviewDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateProductReviewCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<ProductReviewDto>> Handle(UpdateProductReviewCommand command, CancellationToken ct)
    {
        var review = await _context.Set<ProductReview>().FirstOrDefaultAsync(r => r.Id == command.Id && r.UserId == command.UserId, ct);
        if (review is null)
        {
            return Result<ProductReviewDto>.Failure(_localizer["Review.NotFound"].Value);
        }

        try
        {
            review.UpdateOwn(command.Request.Rating, command.Request.Comment, command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<ProductReviewDto>.Failure(ex.Message);
        }

        await _context.SaveChangesAsync(ct);

        var user = await _context.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.Id.ToString() == command.UserId, ct);

        var dto = new ProductReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            UserFullName = user?.FullName ?? "N/A",
            Rating = review.Rating,
            Comment = review.Comment,
            IsApproved = review.IsApproved,
            CreatedAt = review.CreatedAt
        };

        return Result<ProductReviewDto>.Success(dto);
    }
}
