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

        var categoryName = await _context.Set<Category>()
            .Where(c => c.Id == command.Request.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);

        if (categoryName is null)
        {
            return Result<ProductDto>.Failure(_localizer["Catalog.Category.NotFound"].Value);
        }

        string? brandName = null;
        if (command.Request.BrandId.HasValue)
        {
            brandName = await _context.Set<Brand>()
                .Where(b => b.Id == command.Request.BrandId.Value)
                .Select(b => b.Name)
                .FirstOrDefaultAsync(ct);

            if (brandName is null)
            {
                return Result<ProductDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value);
            }
        }

        string? imageUrl = null;
        if (command.Request.Images is { Count: > 0 })
        {
            var file = command.Request.Images.First();
            var uploadResult = await _fileStorage.SaveAsync(file, ImagesFolder, ct);
            if (!uploadResult.Succeeded)
            {
                return Result<ProductDto>.Failure(_localizer["Catalog.Product.CreatedButImageUploadFailed", uploadResult.Error!].Value);
            }

            imageUrl = uploadResult.Data!.RelativeUrl;
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

            if (!string.IsNullOrEmpty(imageUrl))
            {
                product.AddImage(imageUrl, command.UserId);
            }
        }
        catch (DomainException ex)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _fileStorage.DeleteAsync(imageUrl, ct);
            }
            return Result<ProductDto>.Failure(LocalizeDomainError(ex));
        }

        _context.Set<Product>().Add(product);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _fileStorage.DeleteAsync(imageUrl, ct);
            }
            return Result<ProductDto>.Failure(_localizer["Catalog.Product.DuplicateSku"].Value);
        }

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            BrandId = product.BrandId,
            BrandName = brandName,
            Images = product.Images
                .OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder)
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain,
                    DisplayOrder = i.DisplayOrder
                })
                .ToList(),
            Variants = product.Variants
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    SKU = v.SKU,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Size = v.Size,
                    Color = v.Color
                })
                .ToList()
        };

        return Result<ProductDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Catalog.ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "ProductImage.NotFound" => _localizer["Catalog.ProductImage.NotFound"].Value,
        "Variant.NotFound" => _localizer["Catalog.Variant.NotFound"].Value,
        _ => ex.Message
    };
}
