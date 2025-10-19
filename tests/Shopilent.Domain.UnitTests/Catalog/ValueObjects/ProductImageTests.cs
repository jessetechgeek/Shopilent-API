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
        result.IsSuccess.Should().BeTrue();
        var productImage = result.Value;
        productImage.ImageKey.Should().Be(imageKey);
        productImage.ThumbnailKey.Should().Be(thumbnailKey);
        productImage.AltText.Should().Be(altText);
        productImage.IsDefault.Should().Be(isDefault);
        productImage.DisplayOrder.Should().Be(displayOrder);
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
        result.IsSuccess.Should().BeTrue();
        var productImage = result.Value;
        productImage.ImageKey.Should().Be(imageKey);
        productImage.ThumbnailKey.Should().Be(thumbnailKey);
        productImage.AltText.Should().BeNull();
        productImage.IsDefault.Should().BeFalse();
        productImage.DisplayOrder.Should().Be(0);
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
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Image Key is required");
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
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Image Key is required");
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
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Image Key is required");
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
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Thumbnail Key is required");
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
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Thumbnail Key is required");
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
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Thumbnail Key is required");
    }

    [Fact]
    public void SetAsDefault_ShouldSetIsDefaultToTrue()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg", isDefault: false);
        productImageResult.IsSuccess.Should().BeTrue();
        var productImage = productImageResult.Value;
        productImage.IsDefault.Should().BeFalse();

        // Act
        productImage.SetAsDefault();

        // Assert
        productImage.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void RemoveDefault_ShouldSetIsDefaultToFalse()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg", isDefault: true);
        productImageResult.IsSuccess.Should().BeTrue();
        var productImage = productImageResult.Value;
        productImage.IsDefault.Should().BeTrue();

        // Act
        productImage.RemoveDefault();

        // Assert
        productImage.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void UpdateDisplayOrder_WithValidOrder_ShouldUpdateOrder()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg", displayOrder: 0);
        productImageResult.IsSuccess.Should().BeTrue();
        var productImage = productImageResult.Value;
        var newOrder = 5;

        // Act
        productImage.UpdateDisplayOrder(newOrder);

        // Assert
        productImage.DisplayOrder.Should().Be(newOrder);
    }

    [Fact]
    public void UpdateDisplayOrder_WithNegativeOrder_ShouldThrowArgumentException()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg");
        productImageResult.IsSuccess.Should().BeTrue();
        var productImage = productImageResult.Value;
        var negativeOrder = -1;

        // Act & Assert
        var action = () => productImage.UpdateDisplayOrder(negativeOrder);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Display order cannot be negative*");
    }

    [Fact]
    public void UpdateDisplayOrder_WithZero_ShouldWork()
    {
        // Arrange
        var productImageResult = ProductImage.Create("image.jpg", "thumb.jpg", displayOrder: 5);
        productImageResult.IsSuccess.Should().BeTrue();
        var productImage = productImageResult.Value;

        // Act
        productImage.UpdateDisplayOrder(0);

        // Assert
        productImage.DisplayOrder.Should().Be(0);
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

        image1Result.IsSuccess.Should().BeTrue();
        image2Result.IsSuccess.Should().BeTrue();

        // Act & Assert
        image1Result.Value.Should().Be(image2Result.Value);
        image1Result.Value.Equals(image2Result.Value).Should().BeTrue();
        image1Result.Value.GetHashCode().Should().Be(image2Result.Value.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentImageKey_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image1.jpg", "thumb.jpg");
        var image2Result = ProductImage.Create("image2.jpg", "thumb.jpg");

        image1Result.IsSuccess.Should().BeTrue();
        image2Result.IsSuccess.Should().BeTrue();

        // Act & Assert
        image1Result.Value.Should().NotBe(image2Result.Value);
    }

    [Fact]
    public void Equality_WithDifferentThumbnailKey_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb1.jpg");
        var image2Result = ProductImage.Create("image.jpg", "thumb2.jpg");

        image1Result.IsSuccess.Should().BeTrue();
        image2Result.IsSuccess.Should().BeTrue();

        // Act & Assert
        image1Result.Value.Should().NotBe(image2Result.Value);
    }

    [Fact]
    public void Equality_WithDifferentAltText_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", "Alt 1");
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", "Alt 2");

        image1Result.IsSuccess.Should().BeTrue();
        image2Result.IsSuccess.Should().BeTrue();

        // Act & Assert
        image1Result.Value.Should().NotBe(image2Result.Value);
    }

    [Fact]
    public void Equality_WithDifferentIsDefault_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", isDefault: true);
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", isDefault: false);

        image1Result.IsSuccess.Should().BeTrue();
        image2Result.IsSuccess.Should().BeTrue();

        // Act & Assert
        image1Result.Value.Should().NotBe(image2Result.Value);
    }

    [Fact]
    public void Equality_WithDifferentDisplayOrder_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", displayOrder: 1);
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", displayOrder: 2);

        image1Result.IsSuccess.Should().BeTrue();
        image2Result.IsSuccess.Should().BeTrue();

        // Act & Assert
        image1Result.Value.Should().NotBe(image2Result.Value);
    }

    [Fact]
    public void Equality_OneWithNullAltTextOneWithout_ShouldNotBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", null);
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", "Alt text");

        image1Result.IsSuccess.Should().BeTrue();
        image2Result.IsSuccess.Should().BeTrue();

        // Act & Assert
        image1Result.Value.Should().NotBe(image2Result.Value);
    }

    [Fact]
    public void Equality_BothWithNullAltText_ShouldBeEqual()
    {
        // Arrange
        var image1Result = ProductImage.Create("image.jpg", "thumb.jpg", null);
        var image2Result = ProductImage.Create("image.jpg", "thumb.jpg", null);

        image1Result.IsSuccess.Should().BeTrue();
        image2Result.IsSuccess.Should().BeTrue();

        // Act & Assert
        image1Result.Value.Should().Be(image2Result.Value);
    }
}