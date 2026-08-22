using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Brands.Commands.UpdateBrand;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, Result<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public UpdateBrandCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _context = context;
        _localizer = localizer;
        _cache = cache;
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
