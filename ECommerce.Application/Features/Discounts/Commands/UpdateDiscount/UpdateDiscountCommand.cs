using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Commands.UpdateDiscount;

public record UpdateDiscountCommand(Guid Id, UpdateDiscountRequest Request, string UserId) : IRequest<Result<DiscountDto>>;
