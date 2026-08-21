using ECommerce.Application.DTOs.Catalog;

namespace ECommerce.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    private const string ImagesFolder = "products";

    public CreateProductCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var normalizedSku = command.Request.SKU?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedSku))
        {
            return Result<ProductDto>.Failure(_localizer["Validation.Required"].Value);
        }

        if (await _context.Set<Product>().AnyAsync(p => p.SKU.ToLower() == normalizedSku.ToLower(), ct))
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Product.DuplicateSku"].Value);
        }

        if (!await _context.Set<Category>().AnyAsync(c => c.Id == command.Request.CategoryId, ct))
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Category.NotFound"].Value);
        }

        if (command.Request.BrandId.HasValue && !await _context.Set<Brand>().AnyAsync(b => b.Id == command.Request.BrandId, ct))
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value);
        }

        Product product;
        try
        {
            product = Product.Create(
                command.Request.Name?.Trim() ?? string.Empty,
                command.Request.Description,
                normalizedSku,
                command.Request.Price,
                command.Request.CompareAtPrice,
                command.Request.StockQuantity,
                command.Request.CategoryId,
                command.Request.BrandId,
                command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<ProductDto>.Failure(LocalizeDomainError(ex));
        }

        _context.Set<Product>().Add(product);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Product.DuplicateSku"].Value);
        }

        if (command.Request.Images is { Count: > 0 })
        {
            var file = command.Request.Images.First();
            var uploadResult = await _fileStorage.SaveAsync(file, ImagesFolder, ct);
            if (!uploadResult.Succeeded)
            {
                return Result<ProductDto>.Failure(_localizer["Catalog.Product.CreatedButImageUploadFailed", uploadResult.Error!].Value);
            }

            product.AddImage(uploadResult.Data!.RelativeUrl, command.UserId);
            await _context.SaveChangesAsync(ct);
        }

        var dto = await _context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Id == product.Id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SKU = p.SKU,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                Images = p.Images
                    .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageDto { Id = i.Id, ImageUrl = i.ImageUrl, IsMain = i.IsMain, DisplayOrder = i.DisplayOrder })
                    .ToList(),
                Variants = p.Variants
                    .Select(v => new ProductVariantDto { Id = v.Id, SKU = v.SKU, Price = v.Price, StockQuantity = v.StockQuantity, Size = v.Size, Color = v.Color })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<ProductDto>.Failure(_localizer["Catalog.Product.NotFound"].Value) : Result<ProductDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Catalog.ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "Variant.NotFound" => _localizer["Catalog.Variant.NotFound"].Value,
        _ => ex.Message
    };
}
