using backend.Enums;

namespace backend.Services;

public static class CatalogRequestParser
{
    public static CatalogSort ParseSort(string? value) => value switch
    {
        "price-low" => CatalogSort.PriceLow,
        "price-high" => CatalogSort.PriceHigh,
        "name" => CatalogSort.Name,
        "name-desc" => CatalogSort.NameDescending,
        "discount-low" => CatalogSort.DiscountLow,
        _ => CatalogSort.DiscountHigh
    };

    public static bool TryParseStoreIds(string? value, out List<int>? storeIds)
    {
        storeIds = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var parsedStoreIds = new HashSet<int>();
        foreach (var token in value.Split(',', StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(token, out var storeId) || storeId <= 0)
            {
                return false;
            }

            parsedStoreIds.Add(storeId);
        }

        storeIds = parsedStoreIds.ToList();
        return true;
    }

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
