namespace ECommerce.Application.Features.Discounts.Commands.DeleteDiscount;

public class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public DeleteDiscountCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _context = context;
        _localizer = localizer;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteDiscountCommand command, CancellationToken ct)
    {
        var discount = await _context.Set<Discount>().FirstOrDefaultAsync(d => d.Id == command.Id, ct);
        if (discount is null)
        {
            return Result.Failure(_localizer["Discount.NotFound"].Value);
        }

        discount.IsDeleted = true;
        await _context.SaveChangesAsync(ct);

        _cache.Remove("discounts:active:all");
        return Result.Success();
    }
}
