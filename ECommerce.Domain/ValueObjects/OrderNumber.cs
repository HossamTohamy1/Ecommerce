using System.Text.RegularExpressions;

namespace ECommerce.Domain.ValueObjects;

public sealed partial record OrderNumber
{
    public string Value { get; }

    private OrderNumber(string value)
    {
        Value = value;
    }

    public static OrderNumber Generate()
        => new($"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}");

    public static OrderNumber Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !FormatRegex().IsMatch(value))
        {
            throw new DomainException("OrderNumber.InvalidFormat", $"'{value}' is not a valid order number.");
        }

        return new OrderNumber(value);
    }

    [GeneratedRegex(@"^ORD-\d{8}-\d{6}$")]
    private static partial Regex FormatRegex();

    public override string ToString() => Value;
}
