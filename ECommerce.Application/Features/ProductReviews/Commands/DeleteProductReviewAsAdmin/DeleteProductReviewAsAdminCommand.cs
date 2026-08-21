namespace ECommerce.Application.Features.ProductReviews.Commands.DeleteProductReviewAsAdmin;

public record DeleteProductReviewAsAdminCommand(Guid Id) : IRequest<Result>;
