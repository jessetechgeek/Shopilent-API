using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Catalog.ValueObjects;

public class ProductImage : ValueObject
{
    public string Url { get; }
    public string? AltText { get; }
    public bool IsDefault { get; private set; }
    public int DisplayOrder { get; private set; }

    // Required by EF Core
    protected ProductImage()
    {
    }
    
    private ProductImage(string url, string? altText, bool isDefault, int displayOrder)
    {
        Url = url;
        AltText = altText;
        IsDefault = isDefault;
        DisplayOrder = displayOrder;
    }

    public static Result<ProductImage> Create(string url, string? altText = null, bool isDefault = false, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure<ProductImage>(Error.Validation(message: "Image URL is required"));

        // You might want to add URL validation here
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return Result.Failure<ProductImage>(Error.Validation(message: "Invalid image URL format"));

        return Result.Success(new ProductImage(url, altText, isDefault, displayOrder));
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void RemoveDefault()
    {
        IsDefault = false;
    }

    public void UpdateDisplayOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Display order cannot be negative");
            
        DisplayOrder = order;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Url;
        if (AltText != null) yield return AltText;
        yield return IsDefault;
        yield return DisplayOrder;
    }
}