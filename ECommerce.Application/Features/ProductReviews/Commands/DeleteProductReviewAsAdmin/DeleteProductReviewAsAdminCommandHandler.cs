namespace ECommerce.Application.Features.ProductReviews.Commands.DeleteProductReviewAsAdmin;

public class DeleteProductReviewAsAdminCommandHandler : IRequestHandler<DeleteProductReviewAsAdminCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteProductReviewAsAdminCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(DeleteProductReviewAsAdminCommand command, CancellationToken ct)
    {
        var review = await _context.Set<ProductReview>().FirstOrDefaultAsync(r => r.Id == command.Id, ct);
        if (review is null)
        {
            return Result.Failure(_localizer["Review.NotFound"].Value);
        }

        review.IsDeleted = true;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
