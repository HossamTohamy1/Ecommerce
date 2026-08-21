using ECommerce.Application.DTOs.Reviews;

namespace ECommerce.Application.Features.ProductReviews.Queries.GetApprovedReviewsForProduct;

public record GetApprovedReviewsForProductQuery(Guid ProductId) : IRequest<List<ProductReviewDto>>;
