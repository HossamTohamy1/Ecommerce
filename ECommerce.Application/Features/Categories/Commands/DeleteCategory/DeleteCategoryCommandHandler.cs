namespace ECommerce.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteCategoryCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        var category = await _context.Set<Category>().FirstOrDefaultAsync(c => c.Id == command.Id, ct);
        if (category is null)
        {
            return Result.Failure(_localizer["Catalog.Category.NotFound"].Value);
        }

        var hasProducts = await _context.Set<Product>().AnyAsync(p => p.CategoryId == command.Id, ct);
        if (hasProducts)
        {
            return Result.Failure(_localizer["Catalog.Category.HasProducts"].Value);
        }

        category.IsDeleted = true;
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
