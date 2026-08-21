namespace ECommerce.Application.Features.Products.Commands.DeleteProductVariant;

public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteProductVariantCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(DeleteProductVariantCommand command, CancellationToken ct)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, ct);

        if (product is null)
        {
            return Result.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        try
        {
            product.RemoveVariant(command.VariantId);
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
        "Variant.NotFound" => _localizer["Catalog.Variant.NotFound"].Value,
        _ => ex.Message
    };
}
