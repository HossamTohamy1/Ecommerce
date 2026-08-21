using ECommerce.Application.DTOs.Reviews;

namespace ECommerce.Application.Features.ProductReviews.Commands.UpdateProductReview;

public record UpdateProductReviewCommand(string UserId, Guid Id, UpdateProductReviewRequest Request) : IRequest<Result<ProductReviewDto>>;
