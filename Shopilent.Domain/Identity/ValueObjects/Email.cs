using System.Text.RegularExpressions;
using Shopilent.Domain.Common;

namespace Shopilent.Domain.Identity.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; private set; }

    protected Email()
    {
    }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        if (!regex.IsMatch(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        if (value.Contains("..") || value.StartsWith(".") || value.EndsWith(".") || 
            value.Contains("@.") || value.Contains(".@") || value.StartsWith("@") ||
            value.Split('@').Length != 2 || !value.Split('@')[1].Contains("."))
        {
            throw new ArgumentException("Invalid email format", nameof(value));
        }

        Value = value.ToLowerInvariant();
    }

    public static Email Create(string value)
    {
        return new Email(value);
    }

    public static bool TryCreate(string value, out Email email)
    {
        try
        {
            email = Create(value);
            return true;
        }
        catch
        {
            email = null;
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}