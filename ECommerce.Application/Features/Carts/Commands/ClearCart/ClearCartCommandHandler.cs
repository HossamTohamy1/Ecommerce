namespace ECommerce.Application.Features.Carts.Commands.ClearCart;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ClearCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ClearCartCommand command, CancellationToken ct)
    {
        var cart = await _context.Set<Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == command.UserId, ct);

        if (cart is null)
        {
            return Result.Success();
        }

        cart.Clear();
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
