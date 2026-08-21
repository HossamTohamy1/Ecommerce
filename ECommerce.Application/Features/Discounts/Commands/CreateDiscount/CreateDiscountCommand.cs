using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Commands.CreateDiscount;

public record CreateDiscountCommand(CreateDiscountRequest Request, string UserId) : IRequest<Result<DiscountDto>>;
