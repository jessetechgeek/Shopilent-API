using System.Text;

namespace Shopilent.API.Common.Extensions;

public static class HttpRequestExtensions
{
    public static async Task<string> GetRawBodyStringAsync(this HttpRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            // Enable buffering to allow multiple reads
            request.EnableBuffering();

            // Reset position to beginning  
            request.Body.Position = 0;

            // Use memory stream to capture body
            using var memoryStream = new MemoryStream();
            await request.Body.CopyToAsync(memoryStream);
            
            // Reset position
            request.Body.Position = 0;
            
            // Convert to string
            var bodyBytes = memoryStream.ToArray();
            return Encoding.UTF8.GetString(bodyBytes);
        }
        catch (Exception)
        {
            // Fallback method
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
    }
}