namespace ECommerce.Application.Features.Discounts.Commands.RemoveProductFromDiscount;

public class RemoveProductFromDiscountCommandHandler : IRequestHandler<RemoveProductFromDiscountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RemoveProductFromDiscountCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(RemoveProductFromDiscountCommand command, CancellationToken ct)
    {
        var discount = await _context.Set<Discount>()
            .Include(d => d.ProductDiscounts)
            .FirstOrDefaultAsync(d => d.Id == command.DiscountId, ct);

        if (discount is null)
        {
            return Result.Failure(_localizer["Discount.NotFound"].Value);
        }

        try
        {
            discount.RemoveProduct(command.ProductId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(LocalizeDomainError(ex));
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Discount.ProductNotAssigned" => _localizer["Discount.ProductNotAssigned"].Value,
        _ => ex.Message
    };
}
