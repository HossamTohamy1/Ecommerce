using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Brands.Queries.GetPagedBrands;

public class GetPagedBrandsQueryHandler : IRequestHandler<GetPagedBrandsQuery, PagedResult<BrandDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPagedBrandsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BrandDto>> Handle(GetPagedBrandsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.Set<Brand>().AsNoTracking().OrderBy(b => b.Name);
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<BrandDto>()
            .ToListAsync(ct);

        return new PagedResult<BrandDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
