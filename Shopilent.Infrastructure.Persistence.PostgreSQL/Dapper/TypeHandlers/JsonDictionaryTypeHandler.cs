using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Dapper.TypeHandlers;

public class JsonDictionaryTypeHandler : SqlMapper.TypeHandler<Dictionary<string, object>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public override Dictionary<string, object> Parse(object value)
    {
        if (value == null || value == DBNull.Value)
        {
            return new Dictionary<string, object>();
        }

        var jsonString = value.ToString();
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString, JsonOptions)
                   ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to deserialize JSON metadata: {ex.Message}. JSON: {jsonString}");
            return new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected error deserializing JSON metadata: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    public override void SetValue(IDbDataParameter parameter, Dictionary<string, object> value)
    {
        if (value == null || value.Count == 0)
        {
            parameter.Value = "{}";
        }
        else
        {
            try
            {
                parameter.Value = JsonSerializer.Serialize(value, JsonOptions);
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to serialize dictionary to JSON: {ex.Message}");
                parameter.Value = "{}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error serializing dictionary: {ex.Message}");
                parameter.Value = "{}";
            }
        }

        parameter.DbType = DbType.String;
    }
}