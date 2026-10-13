using backend.DTOs;

namespace backend.Services;

public class BasketService
{
    public const int MaxItems = 100;
    public const int MaxQuantity = 1000;

    private static readonly List<string> StrictnessWords = new() { "broad", "normal", "strict" };

    // WHY: every problem is collected into one answer, so a stale cart is fixed in one go.
    public List<string> FindProblems(BasketRequestDto request, List<int> knownItemIds)
    {
        if (request.Items is null || request.Items.Count == 0)
            return new List<string> { "The list is empty." };

        var problems = new List<string>();

        if (request.Items.Count > MaxItems)
            problems.Add($"The list has more than {MaxItems} entries.");

        // WHY: any word other than the three is refused, so the caller can never send a raw number.
        if (request.Strictness is not null && !StrictnessWords.Contains(request.Strictness.ToLower()))
            problems.Add("Strictness must be one of: broad, normal, strict.");

        // WHY: every entry is checked before merging, so a bad copy can't hide inside a summed quantity.
        foreach (var item in request.Items)
        {
            if (item.Quantity < 1)
                problems.Add($"Item {item.ItemId}: quantity must be at least 1.");

            if (item.Quantity > MaxQuantity)
                problems.Add($"Item {item.ItemId}: quantity must be at most {MaxQuantity}.");

            if (!knownItemIds.Contains(item.ItemId))
                problems.Add($"Item {item.ItemId} does not exist.");
        }

        return problems;
    }

    public BasketStrictness ParseStrictness(string? word)
    {
        if (word is null) return BasketStrictness.Normal;
        return Enum.Parse<BasketStrictness>(word, true);
    }

    // WHY: the server owns the three numbers, so they can be tuned here without the frontend knowing them.
    public double BarFor(BasketStrictness strictness)
    {
        if (strictness == BasketStrictness.Broad) return 0.2;
        if (strictness == BasketStrictness.Strict) return 0.5;
        return 0.3;
    }

    // WHY: duplicates become one entry, kept in the order they first appeared so rows stay predictable.
    public List<MergedBasketItem> MergeDuplicates(List<BasketRequestItemDto> items)
    {
        return items
            .GroupBy(i => i.ItemId)
            .Select((group, index) => new MergedBasketItem(index + 1, group.Key, group.Sum(i => i.Quantity)))
            .ToList();
    }
}
