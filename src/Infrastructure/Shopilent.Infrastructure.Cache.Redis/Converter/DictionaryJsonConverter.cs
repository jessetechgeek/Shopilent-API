using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Shopilent.Infrastructure.Cache.Redis.Converter;

public class DictionaryJsonConverter : JsonConverter<Dictionary<string, object>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<string, object>? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);

            // Handle JsonElement objects specifically
            if (kvp.Value is System.Text.Json.JsonElement jsonElement)
            {
                WriteJsonElement(writer, jsonElement);
            }
            else
            {
                // For other types, let the serializer handle it
                serializer.Serialize(writer, kvp.Value);
            }
        }

        writer.WriteEndObject();
    }

    private void WriteJsonElement(JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);
                    WriteJsonElement(writer, property.Value);
                }

                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteJsonElement(writer, item);
                }

                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteValue(element.GetString());
                break;

            case JsonValueKind.Number:
                if (element.TryGetInt64(out long longValue))
                {
                    writer.WriteValue(longValue);
                }
                else
                {
                    writer.WriteValue(element.GetDouble());
                }

                break;

            case JsonValueKind.True:
                writer.WriteValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNull();
                break;

            default:
                writer.WriteNull();
                break;
        }
    }

    public override Dictionary<string, object>? ReadJson(JsonReader reader, Type objectType,
        Dictionary<string, object>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.TokenType != JsonToken.StartObject)
            throw new JsonSerializationException($"Expected start of object but got {reader.TokenType}");

        var result = existingValue ?? new Dictionary<string, object>();

        while (reader.Read() && reader.TokenType != JsonToken.EndObject)
        {
            if (reader.TokenType != JsonToken.PropertyName)
                throw new JsonSerializationException($"Expected property name but got {reader.TokenType}");

            var propertyName = reader.Value?.ToString() ?? string.Empty;

            if (!reader.Read())
                throw new JsonSerializationException("Unexpected end of JSON while reading property value");

            result[propertyName] = ReadJsonValue(reader, serializer);
        }

        return result;
    }

    private object ReadJsonValue(JsonReader reader, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                return serializer.Deserialize<Dictionary<string, object>>(reader) ?? new Dictionary<string, object>();

            case JsonToken.StartArray:
                return serializer.Deserialize<List<object>>(reader) ?? new List<object>();

            case JsonToken.Integer:
                return Convert.ToInt64(reader.Value ?? 0);

            case JsonToken.Float:
                return Convert.ToDouble(reader.Value ?? 0);

            case JsonToken.Boolean:
                return Convert.ToBoolean(reader.Value ?? false);

            case JsonToken.String:
                return reader.Value?.ToString() ?? string.Empty;

            case JsonToken.Null:
                return null;

            default:
                return reader.Value ?? null;
        }
    }
}