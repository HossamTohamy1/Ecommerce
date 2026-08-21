using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Brands.Commands.CreateBrand;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, Result<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    private const string LogoFolder = "brands";

    public CreateBrandCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _fileStorage = fileStorage;
        _localizer = localizer;
    }

    public async Task<Result<BrandDto>> Handle(CreateBrandCommand command, CancellationToken ct)
    {
        var normalizedName = command.Request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result<BrandDto>.Failure(_localizer["Validation.Required"].Value);
        }

        if (await _context.Set<Brand>().AnyAsync(b => b.Name.ToLower() == normalizedName.ToLower(), ct))
        {
            return Result<BrandDto>.Failure(_localizer["Catalog.Brand.DuplicateName"].Value);
        }

        var brand = Brand.Create(normalizedName, command.UserId);
        string? uploadedLogoUrl = null;

        if (command.Request.Logo is not null)
        {
            var uploadResult = await _fileStorage.SaveAsync(command.Request.Logo, LogoFolder, ct);
            if (!uploadResult.Succeeded)
            {
                return Result<BrandDto>.Failure(uploadResult.Error!);
            }

            uploadedLogoUrl = uploadResult.Data!.RelativeUrl;
            brand.SetLogo(uploadedLogoUrl, command.UserId);
        }

        _context.Set<Brand>().Add(brand);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (uploadedLogoUrl != null)
            {
                await _fileStorage.DeleteAsync(uploadedLogoUrl, ct);
            }
            return Result<BrandDto>.Failure(_localizer["Catalog.Brand.DuplicateName"].Value);
        }

        var dto = await _context.Set<Brand>()
            .AsNoTracking()
            .Where(b => b.Id == brand.Id)
            .ProjectToType<BrandDto>()
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<BrandDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value) : Result<BrandDto>.Success(dto);
    }
}
