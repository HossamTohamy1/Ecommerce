using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Queries.GetDiscountById;

public record GetDiscountByIdQuery(Guid Id) : IRequest<Result<DiscountDto>>;
