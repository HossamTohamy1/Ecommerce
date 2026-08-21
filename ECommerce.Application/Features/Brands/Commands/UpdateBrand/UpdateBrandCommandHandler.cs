using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Brands.Commands.UpdateBrand;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, Result<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateBrandCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<BrandDto>> Handle(UpdateBrandCommand command, CancellationToken ct)
    {
        var brand = await _context.Set<Brand>().FirstOrDefaultAsync(b => b.Id == command.Id, ct);
        if (brand is null)
        {
            return Result<BrandDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value);
        }

        var normalizedName = command.Request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result<BrandDto>.Failure(_localizer["Validation.Required"].Value);
        }

        if (await _context.Set<Brand>().AnyAsync(b => b.Name.ToLower() == normalizedName.ToLower() && b.Id != command.Id, ct))
        {
            return Result<BrandDto>.Failure(_localizer["Catalog.Brand.DuplicateName"].Value);
        }

        try
        {
            brand.UpdateDetails(normalizedName, command.Request.IsActive, command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<BrandDto>.Failure(ex.Message);
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<BrandDto>.Failure(_localizer["Catalog.Brand.DuplicateName"].Value);
        }

        var dto = await _context.Set<Brand>()
            .AsNoTracking()
            .Where(b => b.Id == command.Id)
            .ProjectToType<BrandDto>()
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<BrandDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value) : Result<BrandDto>.Success(dto);
    }
}
