using System.Text.Json;

namespace scraper;

// returns null if there is no such element in Json
public static class JsonExtensions
{
    public static decimal? GetNullableDecimal(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null
            ? prop.GetDecimal()
            : null;
    }
}