using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Queries.GetAllDiscounts;

public record GetAllDiscountsQuery : IRequest<List<DiscountDto>>;
