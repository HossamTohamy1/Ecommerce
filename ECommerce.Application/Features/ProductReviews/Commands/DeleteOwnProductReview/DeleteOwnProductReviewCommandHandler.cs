namespace ECommerce.Application.Features.ProductReviews.Commands.DeleteOwnProductReview;

public class DeleteOwnProductReviewCommandHandler : IRequestHandler<DeleteOwnProductReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteOwnProductReviewCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(DeleteOwnProductReviewCommand command, CancellationToken ct)
    {
        var review = await _context.Set<ProductReview>().FirstOrDefaultAsync(r => r.Id == command.Id && r.UserId == command.UserId, ct);
        if (review is null)
        {
            return Result.Failure(_localizer["Review.NotFound"].Value);
        }

        review.IsDeleted = true;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
