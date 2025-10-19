using System.Text.RegularExpressions;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Identity.Errors;

namespace Shopilent.Domain.Identity.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; private set; }

    protected Email()
    {
    }

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(UserErrors.EmailRequired);

        var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        if (!regex.IsMatch(value))
            return Result.Failure<Email>(UserErrors.InvalidEmailFormat);

        if (value.Contains("..") || value.StartsWith(".") || value.EndsWith(".") || 
            value.Contains("@.") || value.Contains(".@") || value.StartsWith("@") ||
            value.Split('@').Length != 2 || !value.Split('@')[1].Contains("."))
        {
            return Result.Failure<Email>(UserErrors.InvalidEmailFormat);
        }

        return Result.Success(new Email(value));
    }

    public static Result<Email> TryCreate(string value)
    {
        return Create(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}