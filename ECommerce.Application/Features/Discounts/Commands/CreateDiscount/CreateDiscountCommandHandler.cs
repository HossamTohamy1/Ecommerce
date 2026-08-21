using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Commands.CreateDiscount;

public class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, Result<DiscountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateDiscountCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<DiscountDto>> Handle(CreateDiscountCommand command, CancellationToken ct)
    {
        var normalizedCode = command.Request.Code?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedCode) &&
            await _context.Set<Discount>().AnyAsync(d => d.Code != null && d.Code.ToLower() == normalizedCode.ToLower(), ct))
        {
            return Result<DiscountDto>.Failure(_localizer["Discount.DuplicateCode"].Value);
        }

        Discount discount;
        try
        {
            discount = Discount.Create(
                command.Request.Name?.Trim() ?? string.Empty,
                normalizedCode,
                command.Request.DiscountType,
                command.Request.Value,
                command.Request.StartDate,
                command.Request.EndDate,
                command.Request.MinimumOrderAmount,
                command.Request.UsageLimit,
                command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<DiscountDto>.Failure(LocalizeDomainError(ex));
        }

        _context.Set<Discount>().Add(discount);

        if (command.Request.ProductIds is { Count: > 0 })
        {
            var validProductIds = await _context.Set<Product>()
                .Where(p => command.Request.ProductIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(ct);

            foreach (var pid in validProductIds.Distinct())
            {
                discount.AssignProduct(pid, command.UserId);
            }
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<DiscountDto>.Failure(_localizer["Discount.DuplicateCode"].Value);
        }

        var dto = await _context.Set<Discount>()
            .AsNoTracking()
            .Where(d => d.Id == discount.Id)
            .Select(d => new DiscountDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                DiscountType = d.DiscountType,
                Value = d.Value,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                MinimumOrderAmount = d.MinimumOrderAmount,
                UsageLimit = d.UsageLimit,
                UsageCount = d.UsageCount,
                IsActive = d.IsActive,
                ProductIds = d.ProductDiscounts.Select(pd => pd.ProductId).ToList()
            })
            .FirstOrDefaultAsync(ct);

        return dto is null ? Result<DiscountDto>.Failure(_localizer["Discount.NotFound"].Value) : Result<DiscountDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Discount.EndDateAfterStartDate" => _localizer["Discount.EndDateAfterStartDate"].Value,
        "Discount.PercentageRange" => _localizer["Discount.PercentageRange"].Value,
        "Discount.ValueMustBePositive" => _localizer["Discount.ValueMustBePositive"].Value,
        _ => ex.Message
    };
}
