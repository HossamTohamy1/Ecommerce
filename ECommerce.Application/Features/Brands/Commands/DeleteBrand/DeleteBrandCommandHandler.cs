namespace ECommerce.Application.Features.Brands.Commands.DeleteBrand;

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public DeleteBrandCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteBrandCommand command, CancellationToken ct)
    {
        var brand = await _context.Set<Brand>().FirstOrDefaultAsync(b => b.Id == command.Id, ct);
        if (brand is null)
        {
            return Result.Failure(_localizer["Catalog.Brand.NotFound"].Value);
        }

        var hasProducts = await _context.Set<Product>().AnyAsync(p => p.BrandId == command.Id, ct);
        if (hasProducts)
        {
            return Result.Failure(_localizer["Catalog.Brand.HasProducts"].Value);
        }

        if (!string.IsNullOrEmpty(brand.LogoUrl))
        {
            await _fileStorage.DeleteAsync(brand.LogoUrl, ct);
        }

        brand.IsDeleted = true;
        await _context.SaveChangesAsync(ct);

        _cache.Remove("catalog:brands");
        return Result.Success();
    }
}
