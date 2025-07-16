// using Shopilent.Application.Abstractions.Imaging;
// using SkiaSharp;
//
// namespace Shopilent.Infrastructure.Imaging;
//
// public class ImageService : IImageService
// {
//     public async Task<OptimizedImageResult> ProcessProductImage(Stream imageStream)
//     {
//         // Read the stream into memory (optional, depending on SKBitmap.Decode)
//         using var memoryStream = new MemoryStream();
//         await imageStream.CopyToAsync(memoryStream);
//         memoryStream.Position = 0;
//
//         // Load the image
//         using var originalBitmap = SKBitmap.Decode(memoryStream);
//
//         // Create result object
//         var result = new OptimizedImageResult
//         {
//             // Generate main product image (1200x1200)
//             MainImage = ResizeAndEncodeToWebP(originalBitmap, 1200, 1200, 85),
//
//             // Generate thumbnail (200x200)
//             Thumbnail = ResizeAndEncodeToWebP(originalBitmap, 200, 200, 75)
//         };
//
//         return result;
//     }
//
//     private Stream ResizeAndEncodeToWebP(SKBitmap original, int maxWidth, int maxHeight, int quality)
//     {
//         // Calculate dimensions while preserving aspect ratio
//         double aspectRatio = (double)original.Width / original.Height;
//         int newWidth, newHeight;
//
//         if (aspectRatio >= 1.0) // Width >= Height (landscape or square)
//         {
//             newWidth = Math.Min(original.Width, maxWidth);
//             newHeight = (int)(newWidth / aspectRatio);
//         }
//         else // Height > Width (portrait)
//         {
//             newHeight = Math.Min(original.Height, maxHeight);
//             newWidth = (int)(newHeight * aspectRatio);
//         }
//
//         // Resize image
//         var resizeInfo = new SKImageInfo(newWidth, newHeight);
//         using var resized = original.Resize(resizeInfo, SKFilterQuality.High);
//
//         // Create square image with white padding if needed
//         using var squareImage = CreateSquareImage(resized, maxWidth, maxHeight);
//
//         // Encode as WebP and write to memory stream
//         var outputStream = new MemoryStream();
//         using (var data = squareImage.Encode(SKEncodedImageFormat.Webp, quality))
//         {
//             data.SaveTo(outputStream);
//         }
//
//         // Reset stream position and return
//         outputStream.Position = 0;
//         return outputStream;
//     }
//
//     private SKImage CreateSquareImage(SKBitmap source, int width, int height)
//     {
//         // Create a square canvas with white background
//         var squareInfo = new SKImageInfo(width, height);
//         using var surface = SKSurface.Create(squareInfo);
//         var canvas = surface.Canvas;
//
//         // Fill with white background
//         canvas.Clear(SKColors.White);
//
//         // Calculate position to center the image
//         float left = (width - source.Width) / 2f;
//         float top = (height - source.Height) / 2f;
//
//         // Draw the resized image centered on the white canvas
//         canvas.DrawBitmap(source, left, top);
//
//         return surface.Snapshot();
//     }
// }


using Shopilent.Application.Abstractions.Imaging;
using SkiaSharp;

namespace Shopilent.Infrastructure.Services.Imaging;

public class ImageService : IImageService
{
    public async Task<ImageResult> ProcessProductImage(Stream imageStream)
    {
        // Read the stream into memory (optional, depending on SKBitmap.Decode)
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        // Load the image
        using var originalBitmap = SKBitmap.Decode(memoryStream);

        // Create result object
        var result = new ImageResult
        {
            // Generate main product image (max dimensions 1200x1200)
            MainImage = ResizeAndEncodeToWebP(originalBitmap, 1600, 1600, 95),

            // Generate thumbnail (minimum 200px on shortest side)
            Thumbnail = ResizeAndEncodeToWebP(originalBitmap, 400, 400, 85, ensureMinimumSize: true)
        };

        return result;
    }

    private Stream ResizeAndEncodeToWebP(SKBitmap original, int maxWidth, int maxHeight, int quality,
        bool ensureMinimumSize = false)
    {
        // Calculate dimensions while preserving aspect ratio
        double aspectRatio = (double)original.Width / original.Height;
        int newWidth, newHeight;

        if (ensureMinimumSize)
        {
            // For thumbnails: ensure minimum 200px on the shortest side
            if (aspectRatio >= 1.0) // Width >= Height (landscape or square)
            {
                // Height is the shorter side
                newHeight = Math.Min(original.Height, maxHeight);
                newHeight = Math.Max(newHeight, maxHeight); // Ensure at least maxHeight (200px)
                newWidth = (int)(newHeight * aspectRatio);
            }
            else // Height > Width (portrait)
            {
                // Width is the shorter side
                newWidth = Math.Min(original.Width, maxWidth);
                newWidth = Math.Max(newWidth, maxWidth); // Ensure at least maxWidth (200px)
                newHeight = (int)(newWidth / aspectRatio);
            }
        }
        else
        {
            if (aspectRatio >= 1.0) // Width >= Height (landscape or square)
            {
                newWidth = Math.Min(original.Width, maxWidth);
                newHeight = (int)(newWidth / aspectRatio);
            }
            else // Height > Width (portrait)
            {
                newHeight = Math.Min(original.Height, maxHeight);
                newWidth = (int)(newHeight * aspectRatio);
            }
        }

        // Resize image
        var resizeInfo = new SKImageInfo(newWidth, newHeight);
        using var resized = original.Resize(resizeInfo, SKFilterQuality.High);
        using var resizedImage = SKImage.FromBitmap(resized);

        // Encode as WebP and write to memory stream
        var outputStream = new MemoryStream();
        using (var data = resizedImage.Encode(SKEncodedImageFormat.Webp, quality))
        {
            data.SaveTo(outputStream);
        }

        // Reset stream position and return
        outputStream.Position = 0;
        return outputStream;
    }
}