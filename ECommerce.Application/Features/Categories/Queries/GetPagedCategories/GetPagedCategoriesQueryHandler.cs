using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Categories.Queries.GetPagedCategories;

public class GetPagedCategoriesQueryHandler : IRequestHandler<GetPagedCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPagedCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CategoryDto>> Handle(GetPagedCategoriesQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _context.Set<Category>().AsNoTracking().OrderBy(c => c.Name);
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<CategoryDto>()
            .ToListAsync(ct);

        return new PagedResult<CategoryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
