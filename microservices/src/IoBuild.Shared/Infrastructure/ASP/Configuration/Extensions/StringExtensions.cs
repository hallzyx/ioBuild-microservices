using System.Text.RegularExpressions;

namespace IoBuild.Shared.Infrastructure.ASP.Configuration.Extensions;

public static partial class StringExtensions
{
    /// <summary>
    /// Converts PascalCase or camelCase to kebab-case.
    /// Example: "AuthenticationController" → "authentication"
    /// </summary>
    public static string ToKebabCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // Remove "Controller" suffix for routes
        value = Regex.Replace(value, "Controller$", "", RegexOptions.IgnoreCase);

        return KebabCaseRegex().Replace(value, "-$1").TrimStart('-').ToLower();
    }

    [GeneratedRegex("(?<=[a-z])([A-Z])|(?<=[A-Z])([A-Z][a-z])")]
    private static partial Regex KebabCaseRegex();
}
