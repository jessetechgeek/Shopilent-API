using System.Security.Cryptography;
using System.Text;

namespace Shopilent.Infrastructure.Payments.Utilities;

internal static class WebhookSecurityUtils
{
    public static bool IsValidTimestamp(long timestamp, int toleranceInMinutes = 5)
    {
        var eventDateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var now = DateTimeOffset.UtcNow;
        var timeDifference = Math.Abs((now - eventDateTime).TotalMinutes);
        
        return timeDifference <= toleranceInMinutes;
    }

    public static string ComputeHmacSha256(string message, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static bool VerifySignature(string payload, string signature, string secret)
    {
        if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        var expectedSignature = ComputeHmacSha256(payload, secret);
        return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
    }

    public static Dictionary<string, string> ParseWebhookHeaders(Dictionary<string, string> headers)
    {
        var result = new Dictionary<string, string>();
        
        if (headers == null) return result;

        foreach (var header in headers)
        {
            var key = header.Key.ToLowerInvariant();
            result[key] = header.Value;
        }

        return result;
    }
}