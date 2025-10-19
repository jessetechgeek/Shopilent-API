using System.Text.RegularExpressions;
using Shopilent.Domain.Catalog.Errors;
using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Catalog.ValueObjects;

public class Slug : ValueObject
{
    public string Value { get; private set; }

    protected Slug()
    {
    }

    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Slug>(CategoryErrors.SlugRequired);

        var slug = Regex.Replace(value, @"[^a-zA-Z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").ToLower();
        slug = Regex.Replace(slug, @"-+", "-");

        if (string.IsNullOrWhiteSpace(slug))
            return Result.Failure<Slug>(CategoryErrors.SlugRequired);

        return Result.Success(new Slug(slug));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}