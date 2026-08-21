using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateCategoryCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
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

        if (command.Request.ParentCategoryId.HasValue &&
            !await _context.Set<Category>().AnyAsync(c => c.Id == command.Request.ParentCategoryId, ct))
        {
            return Result<CategoryDto>.Failure(_localizer["Catalog.Category.ParentNotFound"].Value);
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

        var dto = await _context.Set<Category>()
            .AsNoTracking()
            .Where(c => c.Id == category.Id)
            .ProjectToType<CategoryDto>()
            .FirstOrDefaultAsync(ct);

        return dto is null
            ? Result<CategoryDto>.Failure(_localizer["Catalog.Category.NotFound"].Value)
            : Result<CategoryDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Category.CannotBeSelfParent" => _localizer["Catalog.Category.CannotBeSelfParent"].Value,
        _ => ex.Message
    };
}
