using System.Text;
using Shopilent.Application.Abstractions.Imaging;
using Shopilent.Infrastructure.IntegrationTests.Common;
using SkiaSharp;

namespace Shopilent.Infrastructure.IntegrationTests.Infrastructure.Services.Imaging;

[Collection("IntegrationTests")]
public class ImageServiceTests : IntegrationTestBase
{
    private IImageService _imageService = null!;

    public ImageServiceTests(IntegrationTestFixture integrationTestFixture)
        : base(integrationTestFixture)
    {
    }

    protected override Task InitializeTestServices()
    {
        _imageService = GetService<IImageService>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ProcessProductImage_WithValidSquareImage_ShouldReturnMainImageAndThumbnail()
    {
        // Arrange
        await ResetDatabaseAsync();

        var testImageStream = CreateTestImageStream(800, 800, SKColors.Blue);

        // Act
        var result = await _imageService.ProcessProductImage(testImageStream);

        // Assert
        result.Should().NotBeNull();
        result.MainImage.Should().NotBeNull();
        result.Thumbnail.Should().NotBeNull();

        result.MainImage.Length.Should().BeGreaterThan(0);
        result.Thumbnail.Length.Should().BeGreaterThan(0);

        // Verify main image dimensions (should be resized to max 1600x1600)
        result.MainImage.Position = 0;
        using var mainImageBitmap = SKBitmap.Decode(result.MainImage);
        mainImageBitmap.Should().NotBeNull();
        mainImageBitmap.Width.Should().BeLessOrEqualTo(1600);
        mainImageBitmap.Height.Should().BeLessOrEqualTo(1600);

        // Verify thumbnail dimensions (should be resized to max 400x400)
        result.Thumbnail.Position = 0;
        using var thumbnailBitmap = SKBitmap.Decode(result.Thumbnail);
        thumbnailBitmap.Should().NotBeNull();
        thumbnailBitmap.Width.Should().BeLessOrEqualTo(400);
        thumbnailBitmap.Height.Should().BeLessOrEqualTo(400);
    }

    [Fact]
    public async Task ProcessProductImage_WithLargeImage_ShouldResizeToMaxDimensions()
    {
        // Arrange
        await ResetDatabaseAsync();

        var testImageStream = CreateTestImageStream(3000, 2000, SKColors.Red);

        // Act
        var result = await _imageService.ProcessProductImage(testImageStream);

        // Assert
        result.Should().NotBeNull();

        // Main image should be resized to fit within 1600x1600
        result.MainImage.Position = 0;
        using var mainImageBitmap = SKBitmap.Decode(result.MainImage);
        mainImageBitmap.Width.Should().BeLessOrEqualTo(1600);
        mainImageBitmap.Height.Should().BeLessOrEqualTo(1600);

        // Should preserve aspect ratio (3000:2000 = 1.5:1)
        var aspectRatio = (double)mainImageBitmap.Width / mainImageBitmap.Height;
        aspectRatio.Should().BeApproximately(1.5, 0.1);
    }

    [Fact]
    public async Task ProcessProductImage_WithPortraitImage_ShouldPreserveAspectRatio()
    {
        // Arrange
        await ResetDatabaseAsync();

        var testImageStream = CreateTestImageStream(600, 1200, SKColors.Green);

        // Act
        var result = await _imageService.ProcessProductImage(testImageStream);

        // Assert
        result.Should().NotBeNull();

        // Check aspect ratio preservation (600:1200 = 0.5:1)
        result.MainImage.Position = 0;
        using var mainImageBitmap = SKBitmap.Decode(result.MainImage);
        var aspectRatio = (double)mainImageBitmap.Width / mainImageBitmap.Height;
        aspectRatio.Should().BeApproximately(0.5, 0.1);

        // Should fit within max dimensions
        mainImageBitmap.Width.Should().BeLessOrEqualTo(1600);
        mainImageBitmap.Height.Should().BeLessOrEqualTo(1600);
    }

    [Fact]
    public async Task ProcessProductImage_WithSmallImage_ShouldNotUpscale()
    {
        // Arrange
        await ResetDatabaseAsync();

        var testImageStream = CreateTestImageStream(300, 200, SKColors.Yellow);

        // Act
        var result = await _imageService.ProcessProductImage(testImageStream);

        // Assert
        result.Should().NotBeNull();

        // Small images should not be upscaled for main image
        result.MainImage.Position = 0;
        using var mainImageBitmap = SKBitmap.Decode(result.MainImage);
        mainImageBitmap.Width.Should().BeLessOrEqualTo(300);
        mainImageBitmap.Height.Should().BeLessOrEqualTo(200);

        // Thumbnail might be upscaled to ensure minimum size (due to ensureMinimumSize logic)
        result.Thumbnail.Position = 0;
        using var thumbnailBitmap = SKBitmap.Decode(result.Thumbnail);
        thumbnailBitmap.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessProductImage_ShouldOutputWebPFormat()
    {
        // Arrange
        await ResetDatabaseAsync();

        var testImageStream = CreateTestImageStream(500, 500, SKColors.Purple);

        // Act
        var result = await _imageService.ProcessProductImage(testImageStream);

        // Assert
        result.Should().NotBeNull();

        // Verify both streams contain WebP format
        result.MainImage.Position = 0;
        var mainImageData = new byte[12];
        await result.MainImage.ReadAsync(mainImageData, 0, 12);

        result.Thumbnail.Position = 0;
        var thumbnailData = new byte[12];
        await result.Thumbnail.ReadAsync(thumbnailData, 0, 12);

        // WebP files start with "RIFF" (first 4 bytes) and "WEBP" (bytes 8-11)
        Encoding.ASCII.GetString(mainImageData, 0, 4).Should().Be("RIFF");
        Encoding.ASCII.GetString(mainImageData, 8, 4).Should().Be("WEBP");

        Encoding.ASCII.GetString(thumbnailData, 0, 4).Should().Be("RIFF");
        Encoding.ASCII.GetString(thumbnailData, 8, 4).Should().Be("WEBP");
    }

    [Fact]
    public async Task ProcessProductImage_WithInvalidImageStream_ShouldThrowException()
    {
        // Arrange
        await ResetDatabaseAsync();

        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("This is not an image"));

        // Act & Assert
        var action = () => _imageService.ProcessProductImage(invalidStream);
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ProcessProductImage_WithEmptyStream_ShouldThrowException()
    {
        // Arrange
        await ResetDatabaseAsync();

        var emptyStream = new MemoryStream();

        // Act & Assert
        var action = () => _imageService.ProcessProductImage(emptyStream);
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ProcessProductImage_WithNullStream_ShouldThrowException()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act & Assert
        var action = () => _imageService.ProcessProductImage(null!);
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ProcessProductImage_WithDifferentImageFormats_ShouldHandleAll()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Test with JPEG format
        var jpegStream = CreateTestImageStream(400, 300, SKColors.Orange, SKEncodedImageFormat.Jpeg);
        var jpegResult = await _imageService.ProcessProductImage(jpegStream);
        jpegResult.Should().NotBeNull();
        jpegResult.MainImage.Should().NotBeNull();
        jpegResult.Thumbnail.Should().NotBeNull();

        // Test with PNG format
        var pngStream = CreateTestImageStream(400, 300, SKColors.Cyan, SKEncodedImageFormat.Png);
        var pngResult = await _imageService.ProcessProductImage(pngStream);
        pngResult.Should().NotBeNull();
        pngResult.MainImage.Should().NotBeNull();
        pngResult.Thumbnail.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessProductImage_WithVeryLargeImage_ShouldProcessWithoutMemoryIssues()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Create a large image (4K resolution)
        var largeImageStream = CreateTestImageStream(4096, 2160, SKColors.Black);

        // Act
        var result = await _imageService.ProcessProductImage(largeImageStream);

        // Assert
        result.Should().NotBeNull();
        result.MainImage.Should().NotBeNull();
        result.Thumbnail.Should().NotBeNull();

        // Large image should be resized down
        result.MainImage.Position = 0;
        using var mainImageBitmap = SKBitmap.Decode(result.MainImage);
        mainImageBitmap.Width.Should().BeLessOrEqualTo(1600);
        mainImageBitmap.Height.Should().BeLessOrEqualTo(1600);
    }

    [Fact]
    public async Task ProcessProductImage_ResultStreams_ShouldBeSeekableAndReadable()
    {
        // Arrange
        await ResetDatabaseAsync();

        var testImageStream = CreateTestImageStream(600, 400, SKColors.Pink);

        // Act
        var result = await _imageService.ProcessProductImage(testImageStream);

        // Assert
        result.Should().NotBeNull();

        // Test main image stream properties
        result.MainImage.CanRead.Should().BeTrue();
        result.MainImage.CanSeek.Should().BeTrue();
        result.MainImage.Length.Should().BeGreaterThan(0);

        // Test thumbnail stream properties
        result.Thumbnail.CanRead.Should().BeTrue();
        result.Thumbnail.CanSeek.Should().BeTrue();
        result.Thumbnail.Length.Should().BeGreaterThan(0);

        // Test that streams can be read multiple times
        result.MainImage.Position = 0;
        var firstRead = new byte[100];
        await result.MainImage.ReadAsync(firstRead, 0, 100);

        result.MainImage.Position = 0;
        var secondRead = new byte[100];
        await result.MainImage.ReadAsync(secondRead, 0, 100);

        firstRead.Should().BeEquivalentTo(secondRead);
    }

    private Stream CreateTestImageStream(int width, int height, SKColor color, SKEncodedImageFormat format = SKEncodedImageFormat.Png)
    {
        var imageInfo = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;

        canvas.Clear(color);

        using var image = surface.Snapshot();
        using var data = image.Encode(format, 100);

        var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Position = 0;

        return stream;
    }
}
