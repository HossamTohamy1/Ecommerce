using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Features.Notifications.Commands.NotifyAdmins;

namespace ECommerce.Application.Features.ProductReviews.Commands.CreateProductReview;

public class CreateProductReviewCommandHandler : IRequestHandler<CreateProductReviewCommand, Result<ProductReviewDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly IMediator _mediator;

    public CreateProductReviewCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer, IMediator mediator)
    {
        _context = context;
        _localizer = localizer;
        _mediator = mediator;
    }

    public async Task<Result<ProductReviewDto>> Handle(CreateProductReviewCommand command, CancellationToken ct)
    {
        if (!await _context.Set<Product>().AnyAsync(p => p.Id == command.Request.ProductId, ct))
        {
            return Result<ProductReviewDto>.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        if (await _context.Set<ProductReview>().AnyAsync(r => r.ProductId == command.Request.ProductId && r.UserId == command.UserId, ct))
        {
            return Result<ProductReviewDto>.Failure(_localizer["Review.AlreadyReviewed"].Value);
        }

        ProductReview review;
        try
        {
            review = ProductReview.Create(command.Request.ProductId, command.UserId, command.Request.Rating, command.Request.Comment, command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<ProductReviewDto>.Failure(ex.Message);
        }

        _context.Set<ProductReview>().Add(review);
        await _context.SaveChangesAsync(ct);

        var product = await _context.Set<Product>().FirstOrDefaultAsync(p => p.Id == command.Request.ProductId, ct);
        await _mediator.Send(new NotifyAdminsCommand(
            NotificationType.NewReview,
            _localizer["Notification.NewReview.Title"].Value,
            _localizer["Notification.NewReview.Message", product?.Name ?? command.Request.ProductId.ToString()].Value,
            "/Reviews/Moderation"), ct);

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
