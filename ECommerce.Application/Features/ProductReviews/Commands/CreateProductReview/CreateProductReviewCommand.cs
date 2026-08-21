using ECommerce.Application.DTOs.Reviews;

namespace ECommerce.Application.Features.ProductReviews.Commands.CreateProductReview;

public record CreateProductReviewCommand(string UserId, CreateProductReviewRequest Request) : IRequest<Result<ProductReviewDto>>;
