using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Commands.CreateDiscount;

public class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, Result<DiscountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public CreateDiscountCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _context = context;
        _localizer = localizer;
        _cache = cache;
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

        _cache.Remove("discounts:active:all");

        var dto = new DiscountDto
        {
            Id = discount.Id,
            Name = discount.Name,
            Code = discount.Code,
            DiscountType = discount.DiscountType,
            Value = discount.Value,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            MinimumOrderAmount = discount.MinimumOrderAmount,
            UsageLimit = discount.UsageLimit,
            UsageCount = discount.UsageCount,
            IsActive = discount.IsActive,
            ProductIds = discount.ProductDiscounts.Select(pd => pd.ProductId).ToList()
        };

        return Result<DiscountDto>.Success(dto);
    }

    private string LocalizeDomainError(DomainException ex) => ex.Code switch
    {
        "Discount.EndDateAfterStartDate" => _localizer["Discount.EndDateAfterStartDate"].Value,
        "Discount.PercentageRange" => _localizer["Discount.PercentageRange"].Value,
        "Discount.ValueMustBePositive" => _localizer["Discount.ValueMustBePositive"].Value,
        _ => ex.Message
    };
}
