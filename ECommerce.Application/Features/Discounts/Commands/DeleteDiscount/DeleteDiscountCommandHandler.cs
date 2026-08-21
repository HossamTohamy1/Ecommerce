namespace ECommerce.Application.Features.Discounts.Commands.DeleteDiscount;

public class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteDiscountCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
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

        return Result.Success();
    }
}
