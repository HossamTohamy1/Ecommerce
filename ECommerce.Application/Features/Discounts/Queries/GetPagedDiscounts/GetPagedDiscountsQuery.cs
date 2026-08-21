using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Queries.GetPagedDiscounts;

public record GetPagedDiscountsQuery(int Page, int PageSize) : IRequest<PagedResult<DiscountDto>>;
