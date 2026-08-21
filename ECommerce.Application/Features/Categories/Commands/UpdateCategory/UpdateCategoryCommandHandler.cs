using ECommerce.Application.DTOs.Catalog;
using Mapster;

namespace ECommerce.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateCategoryCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        var category = await _context.Set<Category>().FirstOrDefaultAsync(c => c.Id == command.Id, ct);
        if (category is null)
        {
            return Result<CategoryDto>.Failure(_localizer["Catalog.Category.NotFound"].Value);
        }

        var normalizedName = command.Request.Name?.Trim() ?? string.Empty;
        var normalizedSlug = command.Request.Slug?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedName) || string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return Result<CategoryDto>.Failure(_localizer["Validation.Required"].Value);
        }

        if (await _context.Set<Category>().AnyAsync(c => (c.Slug.ToLower() == normalizedSlug || c.Name.ToLower() == normalizedName.ToLower()) && c.Id != command.Id, ct))
        {
            return Result<CategoryDto>.Failure(_localizer["Catalog.Category.DuplicateSlug"].Value);
        }

        try
        {
            category.UpdateDetails(normalizedName, normalizedSlug, command.Request.ParentCategoryId, command.Request.IsActive, command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<CategoryDto>.Failure(LocalizeDomainError(ex));
        }

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
            .Where(c => c.Id == command.Id)
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
