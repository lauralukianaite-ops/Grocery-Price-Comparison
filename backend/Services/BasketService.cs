using System.Linq.Expressions;
using backend.Data;
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
    public const decimal DefaultCostPerKm = 0.2m;
    public const double DefaultMaxKm = 100;
    
    private static readonly List<string> StrictnessWords = new() { "broad", "normal", "strict" };

    private readonly IItemsRepository _itemsRepository;
    private readonly IBasketsRepository _basketsRepository;
    private readonly IFakeDiscountService _fakeDiscountService;
    private readonly IItemSimilarityService _similarityService;
    private readonly IDistanceProvider _distanceProvider;

    public BasketService(IItemsRepository itemsRepository, IBasketsRepository basketsRepository, IFakeDiscountService fakeDiscountService, IItemSimilarityService similarityService, IDistanceProvider distanceProvider)
    {
        _itemsRepository = itemsRepository;
        _basketsRepository = basketsRepository;
        _fakeDiscountService = fakeDiscountService;
        _similarityService = similarityService;
        _distanceProvider = distanceProvider;
    }

    public async Task<List<string>> FindProblemsAsync(BasketRequestDto request)
    {
        var items = await _itemsRepository.GetItemsAsync();
        return FindProblems(request, items.Select(i => i.Id).ToList());
    }

    public async Task<BasketResult> CreateAsync(BasketRequestDto request)
    {
        // One "now" for the price window, the verdicts and the snapshot date, so they never disagree.
        var now = DateTime.UtcNow;
        var items = await _itemsRepository.GetItemsAsync();
        var freshPrices = await _itemsRepository.GetNewestPricesWithStoreAndItemSinceAsync(now.Date.AddDays(-FreshnessDays), request.BannedIds);

        var strictness = ParseStrictness(request.Strictness);
        var rows = MergeDuplicates(request.Items!);
        var baskets = CompleteBaskets(items, freshPrices, rows, ThresholdFor(strictness));
        if (baskets.Count == 0) return new BasketResult(BasketOutcome.NoCompleteBasket, null);
        
        var locations = await _basketsRepository.GetAllLocationsAsync();
        
        // filter which geographically are near enough 

        var nearBaskets = new List<Basket>();
        var shops = new List<StoreLocation>();

        if (request.Latitude is not null && request.Longitude is not null) {
            foreach (var candidate in baskets)
            {
                var shop = ShopInRange(candidate, locations, request.Latitude.Value, request.Longitude.Value,
                    request.MaxKm ?? DefaultMaxKm);
                if (shop is null) continue;
                nearBaskets.Add(candidate);
                shops.Add(shop);
            }
            if (shops.Count == 0) return new BasketResult(BasketOutcome.NoStoreInRange, null);
            
            try 
            {
                var distances = await _distanceProvider.RoadDistanceKmAsync(request.Latitude.Value, request.Longitude.Value, shops);
                baskets = AddTravel(nearBaskets, shops, distances, request.CostPerKm ?? DefaultCostPerKm, request.MaxKm ?? DefaultMaxKm);
            }
            catch (HttpRequestException)
            {
                return new BasketResult(BasketOutcome.DistanceUnavailable, null);
            }
        }

        var winner = WinnerBasket(baskets);
        // Complete baskets existed, so an empty list here means the road distance cut removed them all.
        if (winner is null) return new BasketResult(BasketOutcome.NoStoreInRange, null);
        
        foreach (var item in winner.Items)
        {
            var history = await _itemsRepository.GetPriceWithStoreByItemIdAsync(item.ItemId);
            item.Verdict = VerdictFor(history, winner.StoreId, now.Date);
        }
        winner.CreatedAt = now;
        winner.Strictness = strictness;
        winner.RequestItems = rows;
        winner.BannedItems = ToBannedItems(request.BannedIds);
        await _basketsRepository.SaveAsync(winner);

        // Answer from the stored row, so create and open can never differ (the database keeps microseconds, .NET keeps more).
        return new BasketResult(BasketOutcome.Created, await GetAsync(winner.Id));
    }

    public StoreLocation? ShopInRange(Basket basket, List<StoreLocation> locations, double latitude, double longitude,
        double maxKm)
    {
        var storeLocations = locations
                .Where(l => l.StoreId == basket.StoreId)
                .ToList();
        var closest = ClosestLocation(storeLocations, latitude, longitude);
        if (closest is null) return null;
        if (GeoDistance.StraightDistance(latitude, longitude, closest.Latitude.Value, closest.Longitude.Value) >
            maxKm) return null;
        return closest;
    }

    public List<Basket> AddTravel(List<Basket> baskets, List<StoreLocation> shops, List<double?> roadKm, decimal costPerKm, double maxKm)
    {
        List<Basket> res = new List<Basket>();
        for (var i = 0; i < baskets.Count; i++)
        {
            if (roadKm[i] > maxKm || roadKm[i] is null) continue;
            baskets[i].DistanceKm = roadKm[i];
            baskets[i].LocationAddress = shops[i].Address;
            baskets[i].TravelCost = Math.Round((decimal)roadKm[i]! * 2 * costPerKm, 2);
            res.Add(baskets[i]);
        }
        return res;
    }

    public Basket? WinnerBasket(List<Basket> baskets)
    {
        return baskets
            .OrderBy(b => b.TravelCost + b.GroceriesTotal)
            .ThenBy(b => b.StoreName)
            .FirstOrDefault();
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
        if (request.CostPerKm < 0)
            problems.Add("The cost per km must be at least 0.");
        if (request.MaxKm <= 0)
            problems.Add("Max kilometers must be over 0.");
        if (request.Latitude is null && request.Longitude is not null)
            problems.Add("Latitude not provided.");
        if (request.Latitude is not null && request.Longitude is null)
            problems.Add("Longitude not provided.");     
        if (request.Latitude < -90 || request.Latitude > 90 || request.Longitude <-180 || request.Longitude > 180)
            problems.Add("Malformed coordinates input.");
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

    // No list and an empty list mean the same thing; a banned id sent twice is stored once.
    public List<BannedItem> ToBannedItems(List<int>? bannedIds)
    {
        return (bannedIds ?? new List<int>())
            .Distinct()
            .Select(id => new BannedItem { ItemId = id })
            .ToList();
    }

    // Every store builds its own basket; the cheapest complete one wins.
    public List<Basket> CompleteBaskets(List<Item> items, List<Price> freshPrices, List<BasketRequestItem> rows, double threshold)
    {
        var stores = freshPrices
            .Select(p => p.Store!)
            .DistinctBy(s => s.Id)
            .ToList();

        return stores
            .Select(store => BuildBasket(store, rows, items, freshPrices, threshold))
            .Where(basket => basket is not null)
            .ToList()!;
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

        basket.GroceriesTotal = basket.Items.Sum(i => i.RowTotal);
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
    public BasketResponseDto ToResponse(Basket basket)
    {
        return new BasketResponseDto(
            basket.Id,
            basket.StoreId,
            basket.StoreName,
            basket.GroceriesTotal,
            basket.TravelCost,
            basket.DistanceKm,
            basket.LocationAddress,
            basket.GroceriesTotal + basket.TravelCost,
            basket.CreatedAt,
            basket.Strictness,
            basket.Items.Select(i => new BasketRowDto(i.Position, i.ItemId, i.ItemName, i.Quantity, i.UnitCost, i.RowTotal, i.Verdict)).ToList(),
            basket.RequestItems.Select(r => new BasketRequestItemDto(r.ItemId, r.Quantity)).ToList(),
            basket.BannedItems.Select(i => i.ItemId).ToList()
        );
    }

    // Returns the closest location for a store chain
    public StoreLocation? ClosestLocation(List<StoreLocation> locations, double userLatitude, double userLongitude)
    {
        return locations
            .Where(loc => loc.Latitude is not null && loc.Longitude is not null)
            .OrderBy(loc => GeoDistance
                .StraightDistance(userLatitude, userLongitude, loc.Latitude.Value, loc.Longitude.Value))
            .FirstOrDefault();
    }
}
