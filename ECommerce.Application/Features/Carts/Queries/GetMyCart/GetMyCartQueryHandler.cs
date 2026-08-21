using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Carts.Queries.GetMyCart;

public class GetMyCartQueryHandler : IRequestHandler<GetMyCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;

    public GetMyCartQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> Handle(GetMyCartQuery request, CancellationToken ct)
    {
        var cart = await _context.Set<Cart>().Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);
        if (cart is null)
        {
            cart = Cart.Create(request.UserId, null, request.UserId);
            _context.Set<Cart>().Add(cart);
            await _context.SaveChangesAsync(ct);
        }

        return await _context.Set<Cart>()
            .AsNoTracking()
            .Where(c => c.Id == cart.Id)
            .Select(c => new CartDto
            {
                Id = c.Id,
                Items = c.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ImageUrl = i.Product.Images.OrderByDescending(img => img.IsMain).ThenBy(img => img.DisplayOrder).Select(img => img.ImageUrl).FirstOrDefault(),
                    ProductVariantId = i.ProductVariantId,
                    VariantLabel = i.ProductVariant != null
                        ? ((i.ProductVariant.Size ?? "") + " " + (i.ProductVariant.Color ?? "")).Trim()
                        : null,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice.Amount
                }).ToList()
            })
            .FirstAsync(ct);
    }
}
