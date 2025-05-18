using Shopilent.Domain.Common;
using Shopilent.Domain.Common.Errors;
using Shopilent.Domain.Common.Results;

namespace Shopilent.Domain.Catalog.ValueObjects;

public class ProductImage : ValueObject
{
    public string ImageKey { get; }
    public string ThumbnailKey { get; }
    public string? AltText { get; }
    public bool IsDefault { get; private set; }
    public int DisplayOrder { get; private set; }

    // Required by EF Core
    protected ProductImage()
    {
    }

    private ProductImage(string imageKey, string thumbnailKey, string? altText, bool isDefault, int displayOrder)
    {
        ImageKey = imageKey;
        ThumbnailKey = thumbnailKey;
        AltText = altText;
        IsDefault = isDefault;
        DisplayOrder = displayOrder;
    }

    public static Result<ProductImage> Create(string imageKey, string thumbnailKey, string? altText = null,
        bool isDefault = false,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
            return Result.Failure<ProductImage>(Error.Validation(message: "Image Key is required"));

        if (string.IsNullOrWhiteSpace(thumbnailKey))
            return Result.Failure<ProductImage>(Error.Validation(message: "Thumbnail Key is required"));

        return Result.Success(new ProductImage(imageKey, thumbnailKey, altText, isDefault, displayOrder));
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
        yield return ImageKey;
        yield return ThumbnailKey;
        if (AltText != null) yield return AltText;
        yield return IsDefault;
        yield return DisplayOrder;
    }
}