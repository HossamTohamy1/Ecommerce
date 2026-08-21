namespace ECommerce.Application.Common.Interfaces;

public interface IDiscountResolver
{
    Task<Discount?> GetActiveDiscountForProductAsync(Guid productId, DateTime now, CancellationToken ct = default);
    Task<Dictionary<Guid, Discount>> GetActiveDiscountsForProductsAsync(IEnumerable<Guid> productIds, DateTime now, CancellationToken ct = default);
    decimal CalculateDiscountAmount(decimal price, Discount discount);
    decimal CalculateDiscountedPrice(decimal price, Discount discount);
}
