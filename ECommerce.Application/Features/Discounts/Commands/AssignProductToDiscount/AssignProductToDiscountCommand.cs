namespace ECommerce.Application.Features.Discounts.Commands.AssignProductToDiscount;

public record AssignProductToDiscountCommand(Guid DiscountId, Guid ProductId, string UserId) : IRequest<Result>;
