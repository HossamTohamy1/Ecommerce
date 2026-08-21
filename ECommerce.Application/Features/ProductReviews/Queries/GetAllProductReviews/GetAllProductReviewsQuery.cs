using ECommerce.Application.DTOs.Reviews;

namespace ECommerce.Application.Features.ProductReviews.Queries.GetAllProductReviews;

public record GetAllProductReviewsQuery : IRequest<List<ProductReviewDto>>;
