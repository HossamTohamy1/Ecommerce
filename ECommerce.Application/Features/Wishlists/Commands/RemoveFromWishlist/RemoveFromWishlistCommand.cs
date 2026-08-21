using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Wishlists.Commands.RemoveFromWishlist;

public record RemoveFromWishlistCommand(string UserId, Guid ProductId) : IRequest<Result<WishlistDto>>;
