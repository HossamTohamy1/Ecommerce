using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Wishlists.Commands.AddToWishlist;

public record AddToWishlistCommand(string UserId, Guid ProductId) : IRequest<Result<WishlistDto>>;
