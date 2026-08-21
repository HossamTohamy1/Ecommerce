namespace ECommerce.Application.Features.Discounts.Commands.RemoveProductFromDiscount;

public record RemoveProductFromDiscountCommand(Guid DiscountId, Guid ProductId) : IRequest<Result>;
