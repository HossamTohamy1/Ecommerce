using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Brands.Queries.GetAllBrands;

public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, List<BrandDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllBrandsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BrandDto>> Handle(GetAllBrandsQuery request, CancellationToken ct)
    {
        return await _context.Set<Brand>()
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ProjectToType<BrandDto>()
            .ToListAsync(ct);
    }
}
