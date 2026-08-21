using ECommerce.Application.DTOs.Reviews;

namespace ECommerce.Application.Features.ProductReviews.Queries.GetPagedProductReviews;

public record GetPagedProductReviewsQuery(int Page, int PageSize) : IRequest<PagedResult<ProductReviewDto>>;
