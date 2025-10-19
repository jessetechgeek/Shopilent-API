using System.Text.Json;

namespace Shopilent.Infrastructure.Cache.Redis.Utilities;

public static class JsonElementHandler
{
    public static object ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dictionary = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    dictionary.Add(property.Name, ConvertJsonElement(property.Value));
                }

                return dictionary;

            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElement(item));
                }

                return list;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt64(out long longValue))
                {
                    return longValue;
                }
                else if (element.TryGetDecimal(out decimal decimalValue))
                {
                    return decimalValue;
                }

                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return null;
        }
    }

    public static Dictionary<string, object> NormalizeJsonElements(Dictionary<string, object> dictionary)
    {
        if (dictionary == null)
            return null;

        var result = new Dictionary<string, object>();

        foreach (var kvp in dictionary)
        {
            if (kvp.Value is JsonElement jsonElement)
            {
                result[kvp.Key] = ConvertJsonElement(jsonElement);
            }
            else if (kvp.Value is Dictionary<string, object> nestedDict)
            {
                result[kvp.Key] = NormalizeJsonElements(nestedDict);
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }
}