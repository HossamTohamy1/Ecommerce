using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ECommerce.Infrastructure.Persistence.Configurations;

internal static class MoneyConversionExtensions
{
    public static PropertyBuilder<Money> HasMoneyConversion(this PropertyBuilder<Money> builder)
    {
        builder.HasConversion(m => m.Amount, v => Money.Of(v));
        builder.Metadata.SetValueComparer(new ValueComparer<Money>(
            (a, b) => a!.Amount == b!.Amount,
            a => a.Amount.GetHashCode(),
            a => Money.Of(a.Amount)));

        return builder;
    }
}
