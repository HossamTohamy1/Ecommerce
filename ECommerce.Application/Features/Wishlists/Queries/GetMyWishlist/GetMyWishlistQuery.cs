using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Wishlists.Queries.GetMyWishlist;

public record GetMyWishlistQuery(string UserId) : IRequest<WishlistDto>;
