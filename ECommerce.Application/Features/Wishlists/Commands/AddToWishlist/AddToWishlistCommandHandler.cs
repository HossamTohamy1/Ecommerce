using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Wishlists.Commands.AddToWishlist;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Result<WishlistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AddToWishlistCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<WishlistDto>> Handle(AddToWishlistCommand command, CancellationToken ct)
    {
        if (!await _context.Set<Product>().AnyAsync(p => p.Id == command.ProductId, ct))
        {
            return Result<WishlistDto>.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        var wishlist = await _context.Set<Wishlist>().Include(w => w.Items).FirstOrDefaultAsync(w => w.UserId == command.UserId, ct);
        if (wishlist is null)
        {
            wishlist = Wishlist.Create(command.UserId, command.UserId);
            _context.Set<Wishlist>().Add(wishlist);
            await _context.SaveChangesAsync(ct);
        }

        wishlist.AddItem(command.ProductId, command.UserId);
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
