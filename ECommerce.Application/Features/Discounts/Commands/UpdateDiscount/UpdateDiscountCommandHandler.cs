using ECommerce.Application.DTOs.Discounts;

namespace ECommerce.Application.Features.Discounts.Commands.UpdateDiscount;

public class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCommand, Result<DiscountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateDiscountCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<DiscountDto>> Handle(UpdateDiscountCommand command, CancellationToken ct)
    {
        var discount = await _context.Set<Discount>().FirstOrDefaultAsync(d => d.Id == command.Id, ct);
        if (discount is null)
        {
            return Result<DiscountDto>.Failure(_localizer["Discount.NotFound"].Value);
        }

        var normalizedCode = command.Request.Code?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedCode) &&
            await _context.Set<Discount>().AnyAsync(d => d.Code != null && d.Code.ToLower() == normalizedCode.ToLower() && d.Id != command.Id, ct))
        {
            return Result<DiscountDto>.Failure(_localizer["Discount.DuplicateCode"].Value);
        }

        try
        {
            discount.UpdateDetails(
                command.Request.Name?.Trim() ?? string.Empty,
                normalizedCode,
                command.Request.DiscountType,
                command.Request.Value,
                command.Request.StartDate,
                command.Request.EndDate,
                command.Request.MinimumOrderAmount,
                command.Request.UsageLimit,
                command.Request.IsActive,
                command.UserId);
        }
        catch (DomainException ex)
        {
            return Result<DiscountDto>.Failure(LocalizeDomainError(ex));
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
            .Where(d => d.Id == command.Id)
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
