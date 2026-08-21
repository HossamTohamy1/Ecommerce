using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Brands.Commands.UploadBrandLogo;

public class UploadBrandLogoCommandHandler : IRequestHandler<UploadBrandLogoCommand, Result<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    private const string LogoFolder = "brands";

    public UploadBrandLogoCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
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

        var dto = await _context.Set<Brand>()
            .AsNoTracking()
            .Where(b => b.Id == command.Id)
            .ProjectToType<BrandDto>()
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<BrandDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value) : Result<BrandDto>.Success(dto);
    }
}
