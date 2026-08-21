using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Commands.UploadProductImages;

public class UploadProductImagesCommandHandler : IRequestHandler<UploadProductImagesCommand, Result<List<ProductImageDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    private const string ImagesFolder = "products";

    public UploadProductImagesCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
    }

    public async Task<Result<List<ProductImageDto>>> Handle(UploadProductImagesCommand command, CancellationToken ct)
    {
        var product = await _context.Set<Product>()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, ct);

        if (product is null)
        {
            return Result<List<ProductImageDto>>.Failure(_localizer["Catalog.Product.NotFound"].Value);
        }

        if (command.Images is null || command.Images.Count == 0)
        {
            return Result<List<ProductImageDto>>.Failure(_localizer["Files.AtLeastOneRequired"].Value);
        }

        var file = command.Images.First();
        var uploadResult = await _fileStorage.SaveAsync(file, ImagesFolder, ct);
        if (!uploadResult.Succeeded)
        {
            return Result<List<ProductImageDto>>.Failure(uploadResult.Error!);
        }

        foreach (var oldImage in product.Images.ToList())
        {
            await _fileStorage.DeleteAsync(oldImage.ImageUrl, ct);
            product.RemoveImage(oldImage.Id);
            _context.Set<ProductImage>().Remove(oldImage);
        }

        var newImage = product.AddImage(uploadResult.Data!.RelativeUrl, command.UserId);
        await _context.SaveChangesAsync(ct);

        return Result<List<ProductImageDto>>.Success(new List<ProductImageDto>
        {
            new() { Id = newImage.Id, ImageUrl = newImage.ImageUrl, IsMain = true, DisplayOrder = 0 }
        });
    }
}
