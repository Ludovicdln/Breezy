using System.Globalization;

namespace Breezy.Extensions;

public static class StringExtensions
{
    public static string ToCamelCase(this string text)
    {
        return text switch
        {
            null => throw new ArgumentNullException(nameof(text)),
            "" => throw new ArgumentNullException(nameof(text), $"{nameof(text)} cannot be empty."),
            _ => text[0].ToString().ToLower(CultureInfo.InvariantCulture) + text.Substring(1)
        };
    }
}