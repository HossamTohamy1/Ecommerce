using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Brands.Commands.UploadBrandLogo;

public class UploadBrandLogoCommandHandler : IRequestHandler<UploadBrandLogoCommand, Result<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    private const string LogoFolder = "brands";

    public UploadBrandLogoCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
        _cache = cache;
    }

    public async Task<Result<BrandDto>> Handle(UploadBrandLogoCommand command, CancellationToken ct)
    {
        var brand = await _context.Set<Brand>().FirstOrDefaultAsync(b => b.Id == command.Id, ct);
        if (brand is null)
        {
            return Result<BrandDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value);
        }

        var uploadResult = await _fileStorage.SaveAsync(command.Logo, LogoFolder, ct);
        if (!uploadResult.Succeeded)
        {
            return Result<BrandDto>.Failure(uploadResult.Error!);
        }

        var oldLogoUrl = brand.LogoUrl;

        brand.SetLogo(uploadResult.Data!.RelativeUrl, command.UserId);
        await _context.SaveChangesAsync(ct);

        if (!string.IsNullOrEmpty(oldLogoUrl))
        {
            await _fileStorage.DeleteAsync(oldLogoUrl, ct);
        }

        _cache.Remove("catalog:brands");

        var productCount = await _context.Set<Product>().CountAsync(p => p.BrandId == command.Id, ct);

        var dto = new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            LogoUrl = brand.LogoUrl,
            ProductCount = productCount
        };

        return Result<BrandDto>.Success(dto);
    }
}
