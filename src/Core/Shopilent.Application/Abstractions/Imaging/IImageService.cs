namespace Shopilent.Application.Abstractions.Imaging;

public interface IImageService
{
    Task<ImageResult> ProcessProductImage(Stream imageStream);
}
