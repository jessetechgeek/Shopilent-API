using System.Text.RegularExpressions;

namespace Shopilent.Infrastructure.Persistence.PostgreSQL.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Converts a string from PascalCase or camelCase to snake_case.
    /// </summary>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // First check if the string is already in snake_case
        if (Regex.IsMatch(input, "^[a-z0-9_]+$"))
            return input;

        // Replace PascalCase and camelCase with snake_case
        var result = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2");
        result = Regex.Replace(result, "([A-Z])([A-Z][a-z])", "$1_$2");
        return result.ToLower();
    }
}