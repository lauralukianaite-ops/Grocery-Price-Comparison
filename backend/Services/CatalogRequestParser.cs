using backend.Enums;

namespace backend.Services;

public static class CatalogRequestParser
{
    public static CatalogSort ParseSort(string? value) => value switch
    {
        "price-low"  => CatalogSort.PriceLow,
        "price-high" => CatalogSort.PriceHigh,
        "name"       => CatalogSort.Name,
        _            => CatalogSort.DiscountHigh
    };

    public static List<int>? ParseItemIds(string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids)) return null;

        return ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var itemId) ? itemId : (int?)null)
            .OfType<int>()
            .Distinct()
            .ToList();
    }
}