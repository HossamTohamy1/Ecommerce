namespace ECommerce.Application.Features.Brands.Commands.DeleteBrand;

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteBrandCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
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

        return Result.Success();
    }
}
