namespace ECommerce.Application.Features.Discounts.Commands.AssignProductToDiscount;

public class AssignProductToDiscountCommandHandler : IRequestHandler<AssignProductToDiscountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AssignProductToDiscountCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(AssignProductToDiscountCommand command, CancellationToken ct)
    {
        var discount = await _context.Set<Discount>()
            .Include(d => d.ProductDiscounts)
            .FirstOrDefaultAsync(d => d.Id == command.DiscountId, ct);

        if (discount is null)
        {
            return Result.Failure(_localizer["Discount.NotFound"].Value);
        }

        if (!await _context.Set<Product>().AnyAsync(p => p.Id == command.ProductId, ct))
        {
            return Result.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        try
        {
            discount.AssignProduct(command.ProductId, command.UserId);
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
        "Discount.ProductAlreadyAssigned" => _localizer["Discount.ProductAlreadyAssigned"].Value,
        _ => ex.Message
    };
}
