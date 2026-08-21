using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Brands.Queries.GetBrandById;

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, Result<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetBrandByIdQueryHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<BrandDto>> Handle(GetBrandByIdQuery request, CancellationToken ct)
    {
        var dto = await _context.Set<Brand>()
            .AsNoTracking()
            .Where(b => b.Id == request.Id)
            .ProjectToType<BrandDto>()
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<BrandDto>.Failure(_localizer["Catalog.Brand.NotFound"].Value) : Result<BrandDto>.Success(dto);
    }
}
