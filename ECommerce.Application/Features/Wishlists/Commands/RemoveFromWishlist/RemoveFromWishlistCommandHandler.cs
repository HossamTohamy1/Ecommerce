using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Wishlists.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Result<WishlistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RemoveFromWishlistCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<WishlistDto>> Handle(RemoveFromWishlistCommand command, CancellationToken ct)
    {
        var wishlist = await _context.Set<Wishlist>()
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == command.UserId, ct);

        if (wishlist is null)
        {
            return Result<WishlistDto>.Failure(_localizer["Wishlist.Empty"].Value);
        }

        wishlist.RemoveItem(command.ProductId);
        await _context.SaveChangesAsync(ct);

        var dto = await _context.Set<Wishlist>()
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
                    ImageUrl = i.Product.Images.Where(img => img.IsMain).Select(img => img.ImageUrl).FirstOrDefault(),
                    Price = i.Product.Price
                }).ToList()
            })
            .FirstAsync(ct);

        return Result<WishlistDto>.Success(dto);
    }
}
