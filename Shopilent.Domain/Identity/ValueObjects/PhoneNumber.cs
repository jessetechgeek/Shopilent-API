using Shopilent.Domain.Common;

namespace Shopilent.Domain.Identity.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Value { get; private set; }

    protected PhoneNumber()
    {
    }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty", nameof(value));

        var digitsOnly = value.StartsWith("+")
            ? "+" + new string(value.Substring(1).Where(char.IsDigit).ToArray())
            : new string(value.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length < 7)
            throw new ArgumentException("Phone number is too short", nameof(value));

        Value = digitsOnly;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}