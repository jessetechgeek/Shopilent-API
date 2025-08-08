using Shopilent.Domain.Catalog.ValueObjects;

namespace Shopilent.Domain.Tests.Catalog.ValueObjects;

public class ProductImageTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateProductImage()
    {
        // Arrange
        var imageKey = "images/product_123.jpg";
        var thumbnailKey = "thumbnails/product_123_thumb.jpg";
        var altText = "Product 123 Image";
        var isDefault = true;
        var displayOrder = 1;

        // Act
        var result = ProductImage.Create(imageKey, thumbnailKey, altText, isDefault, displayOrder);

        // Assert
        Assert.True(result.IsSuccess);
        var productImage = result.Value;
        Assert.Equal(imageKey, productImage.ImageKey);
        Assert.Equal(thumbnailKey, productImage.ThumbnailKey);
        Assert.Equal(altText, productImage.AltText);
        Assert.Equal(isDefault, productImage.IsDefault);
        Assert.Equal(displayOrder, productImage.DisplayOrder);
    }

    [Fact]
    public void Create_WithMinimalParameters_ShouldCreateProductImageWithDefaults()
    {
        // Arrange
        var imageKey = "images/product_123.jpg";
        var thumbnailKey = "thumbnails/product_123_thumb.jpg";

        // Act
        var result = ProductImage.Create(imageKey, thumbnailKey);

        // Assert
        Assert.True(result.IsSuccess);
        var productImage = result.Value;
        Assert.Equal(imageKey, productImage.ImageKey);
        Assert.Equal(thumbnailKey, productImage.ThumbnailKey);
        Assert.Null(productImage.AltText);
        Assert.False(productImage.IsDefault);
        Assert.Equal(0, productImage.DisplayOrder);
    }

    [Fact]
    public void Create_WithEmptyImageKey_ShouldReturnFailure()
    {
        // Arrange
        var imageKey = string.Empty;
        var thumbnailKey = "thumbnails/product_123_thumb.jpg";

        // Act
        var result = ProductImage.Create(imageKey, thumbnailKey);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Image Key is required", result.Error.Message);
    }

    [Fact]
    public void Create_WithNullImageKey_ShouldReturnFailure()
    {
        // Arrange
        string imageKey = null;
        var thumbnailKey = "thumbnails/product_123_thumb.jpg";

        // Act
        var result = ProductImage.Create(imageKey, thumbnailKey);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Image Key is required", result.Error.Message);
    }

    [Fact]
    public void Create_WithWhitespaceImageKey_ShouldReturnFailure()
    {
        // Arrange
        var imageKey = "   ";
        var thumbnailKey = "thumbnails/product_123_thumb.jpg";

        // Act
        var result = ProductImage.Create(imageKey, thumbnailKey);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Image Key is required", result.Error.Message);
    }

    [Fact]
    public void Create_WithEmptyThumbnailKey_ShouldReturnFailure()
    {
        // Arrange
        var imageKey = "images/product_123.jpg";
        var thumbnailKey = string.Empty;

        // Act
        var result = ProductImage.Create(imageKey, thumbnailKey);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Thumbnail Key is required", result.Error.Message);
    }

    [Fact]
    public void Create_WithNullThumbnailKey_ShouldReturnFailure()
    {
        // Arrange
        var imageKey = "images/product_123.jpg";
        string thumbnailKey = null;

        // Act
        var result = ProductImage.Create(imageKey, thumbnailKey);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Thumbnail Key is required", result.Error.Message);
    }

    [Fact]
    public void Create_WithWhitespaceThumbnailKey_ShouldReturnFailure()
    {
        // Arrange
        var imageKey = "images/product_123.jpg";
        var thumbnailKey = "   ";

        // Act
        var result = ProductImage.Create(imageKey, thumbnailKey);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Thumbnail Key is required", result.Error.Message);
    }

    [Fact]
    public void SetAsDefault_ShouldSetIsDefaultToTrue()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg", isDefault: false);
        Assert.True(productImageResult.IsSuccess);
        var productImage = productImageResult.Value;
        Assert.False(productImage.IsDefault);

        // Act
        productImage.SetAsDefault();

        // Assert
        Assert.True(productImage.IsDefault);
    }

    [Fact]
    public void RemoveDefault_ShouldSetIsDefaultToFalse()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg", isDefault: true);
        Assert.True(productImageResult.IsSuccess);
        var productImage = productImageResult.Value;
        Assert.True(productImage.IsDefault);

        // Act
        productImage.RemoveDefault();

        // Assert
        Assert.False(productImage.IsDefault);
    }

    [Fact]
    public void UpdateDisplayOrder_WithValidOrder_ShouldUpdateOrder()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg", displayOrder: 0);
        Assert.True(productImageResult.IsSuccess);
        var productImage = productImageResult.Value;
        var newOrder = 5;

        // Act
        productImage.UpdateDisplayOrder(newOrder);

        // Assert
        Assert.Equal(newOrder, productImage.DisplayOrder);
    }

    [Fact]
    public void UpdateDisplayOrder_WithNegativeOrder_ShouldThrowArgumentException()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg");
        Assert.True(productImageResult.IsSuccess);
        var productImage = productImageResult.Value;
        var negativeOrder = -1;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            productImage.UpdateDisplayOrder(negativeOrder));
        
        Assert.Contains("Display order cannot be negative", exception.Message);
    }

    [Fact]
    public void UpdateDisplayOrder_WithZero_ShouldWork()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg", displayOrder: 5);
        Assert.True(productImageResult.IsSuccess);
        var productImage = productImageResult.Value;

        // Act
        productImage.UpdateDisplayOrder(0);

        // Assert
        Assert.Equal(0, productImage.DisplayOrder);
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var imageKey = "image.jpg";
        var thumbnailKey = "thumb.jpg";
        var altText = "Alt text";
        var isDefault = true;
        var displayOrder = 1;

        var image1Result = ProductImage.Create(imageKey, thumbnailKey, altText, isDefault, displayOrder);
        var image2Result = ProductImage.Create(imageKey, thumbnailKey, altText, isDefault, displayOrder);

        Assert.True(image1Result.IsSuccess);
        Assert.True(image2Result.IsSuccess);

        // Act & Assert
        Assert.Equal(image1Result.Value, image2Result.Value);
        Assert.True(image1Result.Value.Equals(image2Result.Value));
        Assert.Equal(image1Result.Value.GetHashCode(), image2Result.Value.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentImageKey_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image1.jpg", "thumb.jpg");
        var image2Result = ProductImage.Create("image2.jpg", "thumb.jpg");

        Assert.True(image1Result.IsSuccess);
        Assert.True(image2Result.IsSuccess);

        // Act & Assert
        Assert.NotEqual(image1Result.Value, image2Result.Value);
    }

    [Fact]
    public void Equality_WithDifferentThumbnailKey_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb1.jpg");
        var image2Result = ProductImage.Create("image.jpg", "thumb2.jpg");

        Assert.True(image1Result.IsSuccess);
        Assert.True(image2Result.IsSuccess);

        // Act & Assert
        Assert.NotEqual(image1Result.Value, image2Result.Value);
    }

    [Fact]
    public void Equality_WithDifferentAltText_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", "Alt 1");
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", "Alt 2");

        Assert.True(image1Result.IsSuccess);
        Assert.True(image2Result.IsSuccess);

        // Act & Assert
        Assert.NotEqual(image1Result.Value, image2Result.Value);
    }

    [Fact]
    public void Equality_WithDifferentIsDefault_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", isDefault: true);
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", isDefault: false);

        Assert.True(image1Result.IsSuccess);
        Assert.True(image2Result.IsSuccess);

        // Act & Assert
        Assert.NotEqual(image1Result.Value, image2Result.Value);
    }

    [Fact]
    public void Equality_WithDifferentDisplayOrder_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", displayOrder: 1);
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", displayOrder: 2);

        Assert.True(image1Result.IsSuccess);
        Assert.True(image2Result.IsSuccess);

        // Act & Assert
        Assert.NotEqual(image1Result.Value, image2Result.Value);
    }

    [Fact]
    public void Equality_OneWithNullAltTextOneWithout_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", null);
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", "Alt text");

        Assert.True(image1Result.IsSuccess);
        Assert.True(image2Result.IsSuccess);

        // Act & Assert
        Assert.NotEqual(image1Result.Value, image2Result.Value);
    }

    [Fact]
    public void Equality_BothWithNullAltText_ShouldBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", null);
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", null);

        Assert.True(image1Result.IsSuccess);
        Assert.True(image2Result.IsSuccess);

        // Act & Assert
        Assert.Equal(image1Result.Value, image2Result.Value);
    }
}