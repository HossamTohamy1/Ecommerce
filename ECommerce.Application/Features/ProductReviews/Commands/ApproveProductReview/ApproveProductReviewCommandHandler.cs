namespace ECommerce.Application.Features.ProductReviews.Commands.ApproveProductReview;

public class ApproveProductReviewCommandHandler : IRequestHandler<ApproveProductReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ApproveProductReviewCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(ApproveProductReviewCommand command, CancellationToken ct)
    {
        var review = await _context.Set<ProductReview>().FirstOrDefaultAsync(r => r.Id == command.Id, ct);
        if (review is null)
        {
            return Result.Failure(_localizer["Review.NotFound"].Value);
        }

        review.Approve();
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
