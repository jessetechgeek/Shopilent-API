using System.Reflection;
using Newtonsoft.Json;
using Shopilent.Domain.Common.Results;
using Shopilent.Infrastructure.Cache.Redis.Models;
using Shopilent.Infrastructure.Cache.Redis.Services;

namespace Shopilent.Infrastructure.Cache.Redis.Converter;

public class ResultConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        // Check if it's a Result<T> type
        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Result<>))
            return true;

        // Check if it's a Result type
        if (objectType == typeof(Result))
            return true;

        return false;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Create a CachedResult<T> instance
        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Get the type parameter of Result<T>
            Type valueType = objectType.GetGenericArguments()[0];

            // Create the appropriate CachedResult<T> type
            Type cachedResultType = typeof(CachedResult<>).MakeGenericType(valueType);

            // Deserialize to CachedResult<T>
            var cachedResult = serializer.Deserialize(reader, cachedResultType);

            // Convert CachedResult<T> to Result<T>
            MethodInfo toResultMethod = cachedResultType.GetMethod("ToResult");
            return toResultMethod.Invoke(cachedResult, null);
        }

        // For non-generic Result, we would need a similar approach
        // But currently not implemented as this is likely not used in your cache
        throw new JsonSerializationException("Deserialization of non-generic Result is not supported");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // Handle Result<T>
        if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type valueType = value.GetType().GetGenericArguments()[0];
            Type cachedResultType = typeof(CachedResult<>).MakeGenericType(valueType);

            // Create CachedResult<T> from Result<T>
            MethodInfo fromResultMethod = cachedResultType.GetMethod("FromResult");
            var cachedResult = fromResultMethod.Invoke(null, new[] { value });

            // Serialize the CachedResult<T>
            serializer.Serialize(writer, cachedResult);
            return;
        }

        // Handle non-generic Result (if needed)
        throw new JsonSerializationException("Serialization of non-generic Result is not supported");
    }
}