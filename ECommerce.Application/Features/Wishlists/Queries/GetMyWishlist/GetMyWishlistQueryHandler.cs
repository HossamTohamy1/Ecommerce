using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Wishlists.Queries.GetMyWishlist;

public class GetMyWishlistQueryHandler : IRequestHandler<GetMyWishlistQuery, WishlistDto>
{
    private readonly IApplicationDbContext _context;

    public GetMyWishlistQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WishlistDto> Handle(GetMyWishlistQuery request, CancellationToken ct)
    {
        var wishlist = await _context.Set<Wishlist>().Include(w => w.Items).FirstOrDefaultAsync(w => w.UserId == request.UserId, ct);
        if (wishlist is null)
        {
            wishlist = Wishlist.Create(request.UserId, request.UserId);
            _context.Set<Wishlist>().Add(wishlist);
            await _context.SaveChangesAsync(ct);
        }

        return await _context.Set<Wishlist>()
            .AsNoTracking()
            .Where(w => w.Id == wishlist.Id)
            .Select(w => new WishlistDto
            {
                Id = w.Id,
                Items = w.Items.Select(i => new WishlistItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ImageUrl = i.Product.Images.OrderByDescending(img => img.IsMain).ThenBy(img => img.DisplayOrder).Select(img => img.ImageUrl).FirstOrDefault(),
                    Price = i.Product.Price
                }).ToList()
            })
            .FirstAsync(ct);
    }
}
