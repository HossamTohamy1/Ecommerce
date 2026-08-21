using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        var dto = await _context.Set<Category>()
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .ProjectToType<CategoryDto>()
            .FirstOrDefaultAsync(ct);

        return dto is null
            ? Result<CategoryDto>.Failure(_localizer["Catalog.Category.NotFound"].Value)
            : Result<CategoryDto>.Success(dto);
    }
}
