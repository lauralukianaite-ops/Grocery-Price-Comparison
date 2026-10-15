using backend.DTOs;
using backend.Entities;
using backend.Enums;
using backend.Repositories;
using backend.Services.Interfaces;

namespace backend.Services;

public class BasketService : IBasketService
{
    public const int MaxRows = 100;
    public const int MaxItemQuantity = 1000;
    public const int FreshnessDays = 14;

    private static readonly List<string> StrictnessWords = new() { "broad", "normal", "strict" };

    private readonly IItemsRepository _itemsRepository;
    private readonly IBasketsRepository _basketsRepository;
    private readonly IFakeDiscountService _fakeDiscountService;
    private readonly IItemSimilarityService _similarityService;

    public BasketService(IItemsRepository itemsRepository, IBasketsRepository basketsRepository, IFakeDiscountService fakeDiscountService, IItemSimilarityService similarityService)
    {
        _itemsRepository = itemsRepository;
        _basketsRepository = basketsRepository;
        _fakeDiscountService = fakeDiscountService;
        _similarityService = similarityService;
    }

    public async Task<List<string>> FindProblemsAsync(BasketRequestDto request)
    {
        var items = await _itemsRepository.GetItemsAsync();
        return FindProblems(request, items.Select(i => i.Id).ToList());
    }

    public async Task<BasketResponseDto?> CreateAsync(BasketRequestDto request)
    {
        // One "now" for the price window, the verdicts and the snapshot date, so they never disagree.
        var now = DateTime.UtcNow;
        var items = await _itemsRepository.GetItemsAsync();
        var freshPrices = await _itemsRepository.GetNewestPricesWithStoreAndItemSinceAsync(now.Date.AddDays(-FreshnessDays));

        var strictness = ParseStrictness(request.Strictness);
        var rows = MergeDuplicates(request.Items!);
        var basket = FindCheapestBasket(items, freshPrices, rows, ThresholdFor(strictness));
        if (basket is null) return null;

        foreach (var item in basket.Items)
        {
            var history = await _itemsRepository.GetPriceWithStoreByItemIdAsync(item.ItemId);
            item.Verdict = VerdictFor(history, basket.StoreId, now.Date);
        }

        basket.CreatedAt = now;
        basket.Strictness = strictness;
        basket.RequestItems = rows;
        await _basketsRepository.SaveAsync(basket);

        // Answer from the stored row, so create and open can never differ (the database keeps microseconds, .NET keeps more).
        return await GetAsync(basket.Id);
    }

    public async Task<BasketResponseDto?> GetAsync(Guid id)
    {
        var basket = await _basketsRepository.GetByIdAsync(id);
        if (basket is null) return null;
        return ToResponse(basket);
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

    // A discount that cannot be proven bad counts as real, so the frontend only shows a badge for exaggerated or false.
    public DiscountVerdict VerdictFor(List<Price> itemPrices, int storeId, DateTime today)
    {
        var flag = _fakeDiscountService
            .FindDiscountFlags(itemPrices, today)
            .FirstOrDefault(f => f.StoreId == storeId);

        if (flag is null) return DiscountVerdict.Real;
        return flag.Verdict;
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

    // Both create and open answer from the stored basket, so the two can never differ.
    private BasketResponseDto ToResponse(Basket basket)
    {
        return new BasketResponseDto(
            basket.Id,
            basket.StoreId,
            basket.StoreName,
            basket.Total,
            basket.CreatedAt,
            basket.Strictness,
            basket.Items.Select(i => new BasketRowDto(i.Position, i.ItemId, i.ItemName, i.Quantity, i.UnitCost, i.RowTotal, i.Verdict)).ToList(),
            basket.RequestItems.Select(r => new BasketRequestItemDto(r.ItemId, r.Quantity)).ToList()
        );
    }
}
