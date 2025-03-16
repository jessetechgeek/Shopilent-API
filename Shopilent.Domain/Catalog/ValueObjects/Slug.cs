using System.Text.RegularExpressions;
using Shopilent.Domain.Common;

namespace Shopilent.Domain.Catalog.ValueObjects;

public class Slug : ValueObject
{
    public string Value { get; private set; }

    protected Slug()
    {
    }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug cannot be empty", nameof(value));

        var slug = Regex.Replace(value, @"[^a-zA-Z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").ToLower();
        slug = Regex.Replace(slug, @"-+", "-");

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty after processing", nameof(value));

        Value = slug;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}