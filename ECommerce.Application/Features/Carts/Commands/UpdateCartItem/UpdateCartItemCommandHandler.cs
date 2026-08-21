using ECommerce.Application.DTOs.Shopping;

namespace ECommerce.Application.Features.Carts.Commands.UpdateCartItem;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, Result<CartDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateCartItemCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand command, CancellationToken ct)
    {
        var cart = await _context.Set<Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == command.UserId, ct);

        if (cart is null)
        {
            return Result<CartDto>.Failure(_localizer["Cart.ItemNotFound"].Value);
        }

        try
        {
            cart.UpdateItemQuantity(command.ItemId, command.Request.Quantity);
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
