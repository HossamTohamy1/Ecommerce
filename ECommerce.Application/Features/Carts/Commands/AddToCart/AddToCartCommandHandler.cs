using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Carts.Commands.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Result<CartDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AddToCartCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<CartDto>> Handle(AddToCartCommand command, CancellationToken ct)
    {
        var product = await _context.Set<Product>().FirstOrDefaultAsync(p => p.Id == command.Request.ProductId, ct);
        if (product is null || !product.IsActive)
        {
            return Result<CartDto>.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        var unitPrice = Money.Of(product.Price);

        if (command.Request.ProductVariantId.HasValue)
        {
            var variant = await _context.Set<ProductVariant>()
                .FirstOrDefaultAsync(v => v.Id == command.Request.ProductVariantId && v.ProductId == command.Request.ProductId, ct);
            if (variant is null)
            {
                return Result<CartDto>.Failure(_localizer["Catalog.Variant.NotFound"].Value);
            }
            unitPrice = Money.Of(variant.Price ?? product.Price);
        }

        var cart = await _context.Set<Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == command.UserId, ct);

        if (cart is null)
        {
            cart = Cart.Create(command.UserId, null, command.UserId);
            _context.Set<Cart>().Add(cart);
        }

        try
        {
            cart.AddOrMergeItem(command.Request.ProductId, command.Request.ProductVariantId, command.Request.Quantity, unitPrice, command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<CartDto>.Failure(LocalizeDomainError(ex));
        }

        await _context.SaveChangesAsync(ct);

        var dto = await _context.Set<Cart>()
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

        return Result<CartDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Cart.ItemNotFound" => _localizer["Cart.ItemNotFound"].Value,
        _ => ex.Message
    };
}
