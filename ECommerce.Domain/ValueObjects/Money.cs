namespace ECommerce.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money Zero { get; } = new(0m);

    public static Money Of(decimal amount)
    {
        if (amount < 0)
        {
            throw new DomainException("Money.NegativeAmount", "A money amount cannot be negative.");
        }

        return new Money(Math.Round(amount, 2, MidpointRounding.AwayFromZero));
    }

    public Money Add(Money other) => Of(Amount + other.Amount);

    public Money Subtract(Money other) => Of(Amount - other.Amount);

    public Money Multiply(int factor)
    {
        if (factor < 0)
        {
            throw new DomainException("Money.NegativeFactor", "A money amount cannot be multiplied by a negative factor.");
        }

        return Of(Amount * factor);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public static Money operator *(Money money, int factor) => money.Multiply(factor);

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;

    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

    public override string ToString() => Amount.ToString("F2");
}
