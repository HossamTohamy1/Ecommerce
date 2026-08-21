namespace ECommerce.Application.Features.ProductReviews.Commands.DeleteOwnProductReview;

public record DeleteOwnProductReviewCommand(string UserId, Guid Id) : IRequest<Result>;
