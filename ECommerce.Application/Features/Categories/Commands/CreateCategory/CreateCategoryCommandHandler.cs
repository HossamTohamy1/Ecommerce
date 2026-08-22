using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public CreateCategoryCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _context = context;
        _localizer = localizer;
        _cache = cache;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        var normalizedName = command.Request.Name?.Trim() ?? string.Empty;
        var normalizedSlug = command.Request.Slug?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedName) || string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return Result<CategoryDto>.Failure(_localizer["Validation.Required"].Value);
        }

        if (await _context.Set<Category>().AnyAsync(c => c.Slug.ToLower() == normalizedSlug || c.Name.ToLower() == normalizedName.ToLower(), ct))
        {
            return Result<CategoryDto>.Failure(_localizer["Catalog.Category.DuplicateSlug"].Value);
        }

        string? parentCategoryName = null;
        if (command.Request.ParentCategoryId.HasValue)
        {
            parentCategoryName = await _context.Set<Category>()
                .Where(c => c.Id == command.Request.ParentCategoryId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(ct);

            if (parentCategoryName is null)
            {
                return Result<CategoryDto>.Failure(_localizer["Catalog.Category.ParentNotFound"].Value);
            }
        }

        Category category;
        try
        {
            category = Category.Create(normalizedName, normalizedSlug, command.Request.ParentCategoryId, command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<CategoryDto>.Failure(LocalizeDomainError(ex));
        }

        _context.Set<Category>().Add(category);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<CategoryDto>.Failure(_localizer["Catalog.Category.DuplicateSlug"].Value);
        }

        _cache.Remove("catalog:categories");

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parentCategoryName,
            ProductCount = 0
        };

        return Result<CategoryDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Category.CannotBeSelfParent" => _localizer["Catalog.Category.CannotBeSelfParent"].Value,
        _ => ex.Message
    };
}
