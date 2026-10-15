using backend.DTOs;
using backend.Entities;
using backend.Services.Interfaces;

namespace backend.Services;

public class BasketService
{
    public const int MaxRows = 100;
    public const int MaxItemQuantity = 1000;

    private static readonly List<string> StrictnessWords = new() { "broad", "normal", "strict" };

    private readonly IFakeDiscountService _fakeDiscountService;
    private readonly IItemSimilarityService _similarityService;

    public BasketService(IFakeDiscountService fakeDiscountService, IItemSimilarityService similarityService)
    {
        _fakeDiscountService = fakeDiscountService;
        _similarityService = similarityService;
    }

    // Every problem is collected into one answer.
    public List<string> FindProblems(BasketRequestDto request, List<int> existingItemIds)
    {
        if (request.Items is null || request.Items.Count == 0)
            return new List<string> { "The list is empty." };

        var problems = new List<string>();

        if (request.Items.Count > MaxRows)
            problems.Add($"The list has more than {MaxRows} rows.");

        if (request.Strictness is not null && !StrictnessWords.Contains(request.Strictness.ToLower()))
            problems.Add("Strictness must be one of: broad, normal, strict.");

        foreach (var item in request.Items)
        {
            if (item.Quantity < 1)
                problems.Add($"Item {item.ItemId}: quantity must be at least 1.");

            if (item.Quantity > MaxItemQuantity)
                problems.Add($"Item {item.ItemId}: quantity must be at most {MaxItemQuantity}.");

            if (!existingItemIds.Contains(item.ItemId))
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
    public double ThresholdFor(BasketStrictness strictness)
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

    // Every store builds its own basket; the cheapest complete one wins.
    public Basket? FindCheapestBasket(List<Item> items, List<Price> freshPrices, List<BasketRequestItem> rows, double threshold)
    {
        var stores = freshPrices
            .Select(p => p.Store!)
            .DistinctBy(s => s.Id)
            .ToList();

        return stores
            .Select(store => BuildBasket(store, rows, items, freshPrices, threshold))
            .Where(basket => basket is not null)
            .OrderBy(basket => basket!.Total)
            .ThenBy(basket => basket!.StoreName)
            .FirstOrDefault();
    }

    // A store that cannot fill every row is out.
    private Basket? BuildBasket(Store store, List<BasketRequestItem> rows, List<Item> items, List<Price> freshPrices, double threshold)
    {
        var storePrices = freshPrices.Where(p => p.StoreId == store.Id).ToList();
        var basket = new Basket { StoreId = store.Id, StoreName = store.Name };

        foreach (var row in rows)
        {
            var item = CheapestItem(row, items, storePrices, threshold);
            if (item is null) return null;
            basket.Items.Add(item);
        }

        basket.Total = basket.Items.Sum(i => i.RowTotal);
        return basket;
    }

    // Same cost -> more similar name, then lower item id, so the shown item never flips between runs.
    private BasketItem? CheapestItem(BasketRequestItem row, List<Item> items, List<Price> storePrices, double threshold)
    {
        var rowItemName = items.First(i => i.Id == row.ItemId).Name;

        var price = storePrices
            .Where(p => _similarityService.CalculateSimilarity(rowItemName, p.Item!.Name) >= threshold)
            .OrderBy(p => p.Cost)
            .ThenByDescending(p => _similarityService.CalculateSimilarity(rowItemName, p.Item!.Name))
            .ThenBy(p => p.ItemId)
            .FirstOrDefault();

        if (price is null)
            return null;

        return new BasketItem
        {
            Position = row.Position,
            ItemId = price.ItemId,
            ItemName = price.Item!.Name,
            Quantity = row.Quantity,
            UnitCost = price.Cost,
            RowTotal = price.Cost * row.Quantity
        };
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
