namespace ECommerce.Application.Features.Products.Commands.SetMainProductImage;

public class SetMainProductImageCommandHandler : IRequestHandler<SetMainProductImageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SetMainProductImageCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(SetMainProductImageCommand command, CancellationToken ct)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, ct);

        if (product is null)
        {
            return Result.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        try
        {
            product.SetMainImage(command.ImageId);
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
        "Catalog.ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        _ => ex.Message
    };
}
