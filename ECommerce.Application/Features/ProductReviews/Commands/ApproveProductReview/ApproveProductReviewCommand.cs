namespace ECommerce.Application.Features.ProductReviews.Commands.ApproveProductReview;

public record ApproveProductReviewCommand(Guid Id) : IRequest<Result>;
