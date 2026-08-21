namespace ECommerce.Application.Features.Discounts.Commands.DeleteDiscount;

public record DeleteDiscountCommand(Guid Id) : IRequest<Result>;
