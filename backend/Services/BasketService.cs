using backend.DTOs;
using backend.Entities;
using backend.Services.Interfaces;

namespace backend.Services;

public class BasketService
{
    public const int MaxItems = 100;
    public const int MaxQuantity = 1000;

    private static readonly List<string> StrictnessWords = new() { "broad", "normal", "strict" };

    private readonly IFakeDiscountService _fakeDiscountService;

    public BasketService(IFakeDiscountService fakeDiscountService)
    {
        _fakeDiscountService = fakeDiscountService;
    }

    // Every problem is collected into one answer.
    public List<string> FindProblems(BasketRequestDto request, List<int> knownItemIds)
    {
        if (request.Items is null || request.Items.Count == 0)
            return new List<string> { "The list is empty." };

        var problems = new List<string>();

        if (request.Items.Count > MaxItems)
            problems.Add($"The list has more than {MaxItems} entries.");

        if (request.Strictness is not null && !StrictnessWords.Contains(request.Strictness.ToLower()))
            problems.Add("Strictness must be one of: broad, normal, strict.");

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

    // The server owns the three numbers, so they can be tuned here without the frontend knowing them.
    public double BarFor(BasketStrictness strictness)
    {
        if (strictness == BasketStrictness.Broad) return 0.2;
        if (strictness == BasketStrictness.Strict) return 0.5;
        return 0.3;
    }

    // A discount that cannot be proven bad counts as real, so the frontend only shows a badge for exaggerated or false.
    public DiscountVerdict VerdictFor(List<Price> itemPrices, int storeId, DateTime today)
    {
        var flag = _fakeDiscountService
            .FindDiscountFlags(itemPrices, today)
            .FirstOrDefault(f => f.StoreId == storeId);

        if (flag is null) return DiscountVerdict.Real;
        return flag.Verdict;
    }

    public List<BasketRequestItem> MergeDuplicates(List<BasketRequestItemDto> items)
    {
        return items
            .GroupBy(i => i.ItemId)
            .Select((group, index) => new BasketRequestItem
            {
                Position = index + 1,
                ItemId = group.Key,
                Quantity = group.Sum(i => i.Quantity)
            })
            .ToList();
    }
}
