namespace ECommerce.Application.Features.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteProductImageCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
    }

    public async Task<Result> Handle(DeleteProductImageCommand command, CancellationToken ct)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, ct);

        if (product is null)
        {
            return Result.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        var image = product.Images.FirstOrDefault(i => i.Id == command.ImageId);
        if (image is null)
        {
            return Result.Failure(_localizer["Catalog.ProductImage.NotFound"].Value);
        }

        var imageUrl = image.ImageUrl;

        try
        {
            product.RemoveImage(command.ImageId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(LocalizeDomainError(ex));
        }

        await _fileStorage.DeleteAsync(imageUrl, ct);
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
