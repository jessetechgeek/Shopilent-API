using System.Text;
using System.Text.Json;
using Shopilent.API.Common.Models;
using Shopilent.Domain.Common.Results;
using Shopilent.Domain.Common.Errors;

namespace Shopilent.API.Common.Services;

public class FilterEncodingService : IFilterEncodingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public Result<ProductFilters> DecodeFilters(string base64EncodedFilters)
    {
        if (string.IsNullOrWhiteSpace(base64EncodedFilters))
        {
            return Result.Failure<ProductFilters>(
                Error.Validation("FilterEncoding.InvalidBase64", "Base64 encoded filters string is required"));
        }

        try
        {
            var base64Bytes = Convert.FromBase64String(base64EncodedFilters);
            var jsonString = Encoding.UTF8.GetString(base64Bytes);

            var filters = JsonSerializer.Deserialize<ProductFilters>(jsonString, JsonOptions);
            
            if (filters == null)
            {
                return Result.Failure<ProductFilters>(
                    Error.Validation("FilterEncoding.DeserializationFailed", "Failed to deserialize filters from JSON"));
            }

            return Result.Success(filters);
        }
        catch (FormatException)
        {
            return Result.Failure<ProductFilters>(
                Error.Validation("FilterEncoding.InvalidBase64Format", "Invalid base64 format"));
        }
        catch (JsonException ex)
        {
            return Result.Failure<ProductFilters>(
                Error.Validation("FilterEncoding.InvalidJson", $"Invalid JSON format: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<ProductFilters>(
                Error.Failure("FilterEncoding.UnexpectedError", $"An unexpected error occurred while decoding filters: {ex.Message}"));
        }
    }

    public string EncodeFilters(ProductFilters filters)
    {
        ArgumentNullException.ThrowIfNull(filters);

        try
        {
            var jsonString = JsonSerializer.Serialize(filters, JsonOptions);
            
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            return Convert.ToBase64String(jsonBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to encode filters: {ex.Message}", ex);
        }
    }
}