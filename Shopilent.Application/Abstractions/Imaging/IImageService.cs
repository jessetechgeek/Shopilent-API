namespace Shopilent.Application.Abstractions.Imaging;

public interface IImageService
{
    public Task<ImageResult> ProcessProductImage(Stream imageStream);
}