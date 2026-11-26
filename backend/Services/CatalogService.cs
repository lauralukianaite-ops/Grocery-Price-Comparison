using backend.DTOs;
using backend.Entities;
using backend.Enums;
using backend.Repositories;
using backend.ValueObjects;
using backend.Services.Interfaces;

namespace backend.Services;

public class CatalogService : ICatalogService
{
    private readonly IItemsRepository _itemsRepository;
    private readonly IStoresRepository _storesRepository;

    public CatalogService(IItemsRepository itemsRepository, IStoresRepository storesRepository)
    {
        _itemsRepository = itemsRepository;
        _storesRepository = storesRepository;
    }

    public async Task<PagedResponse<CatalogItemDto>> GetCatalogAsync(CatalogQuery query)
    {
        var normalized = query with
        {
            Page = Math.Max(query.Page, 1),
            PageSize = Math.Clamp(query.PageSize, 1, 100),
            Store = string.IsNullOrWhiteSpace(query.Store) ? null : query.Store?.ToLower()
        };

        var latestPrices = await _itemsRepository.GetLatestPricesWithStoreAsync(normalized.Store, normalized.StoreIds);

        var filteredItems = latestPrices
            .Where(price => price.Item is not null)
            .Select(price => price.Item!)
            .DistinctBy(item => item.Id)
            .ToList();

        if (normalized.ItemIds is not null)
        {
            var selectedIds = normalized.ItemIds.ToHashSet();
            filteredItems = filteredItems.Where(item => selectedIds.Contains(item.Id)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(normalized.Search))
        {
            var term = TextSimilarity.Normalize(normalized.Search);
            filteredItems = filteredItems
                .Where(item => TextSimilarity.Normalize(item.Name).Contains(term))
                .ToList();
        }

        var priceStats = latestPrices
            .GroupBy(price => price.ItemId)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    BestPrice = group.Min(price => price.Cost),
                    BestDiscount = group.Max(price =>
                        price.RetailCost is > 0m && price.RetailCost > price.Cost
                            ? (price.RetailCost.Value - price.Cost) / price.RetailCost.Value
                            : 0m)
                });

        var joined = filteredItems
            .Where(item => priceStats.ContainsKey(item.Id))
            .Select(item => new
            {
                Item = item,
                BestPrice = priceStats[item.Id].BestPrice,
                BestDiscount = priceStats[item.Id].BestDiscount
            });

        if (normalized.MinPrice is not null)
        {
            joined = joined.Where(entry => entry.BestPrice >= normalized.MinPrice.Value);
        }

        if (normalized.MaxPrice is not null)
        {
            joined = joined.Where(entry => entry.BestPrice <= normalized.MaxPrice.Value);
        }

        if (normalized.DiscountedOnly)
        {
            joined = joined.Where(entry => entry.BestDiscount > 0);
        }

        var sorted = normalized.Sort switch
        {
            CatalogSort.PriceLow => joined.OrderBy(entry => entry.BestPrice)
                .ThenBy(entry => entry.Item.Name)
                .ThenBy(entry => entry.Item.Id),
            CatalogSort.PriceHigh => joined.OrderByDescending(entry => entry.BestPrice)
                .ThenBy(entry => entry.Item.Name)
                .ThenBy(entry => entry.Item.Id),
            CatalogSort.Name => joined.OrderBy(entry => entry.Item.Name)
                .ThenBy(entry => entry.Item.Id),
            CatalogSort.NameDescending => joined.OrderByDescending(entry => entry.Item.Name)
                .ThenBy(entry => entry.Item.Id),
            CatalogSort.DiscountLow => joined.OrderBy(entry => entry.BestDiscount)
                .ThenBy(entry => entry.BestPrice)
                .ThenBy(entry => entry.Item.Name)
                .ThenBy(entry => entry.Item.Id),
            _ => joined.OrderByDescending(entry => entry.BestDiscount)
                .ThenBy(entry => entry.BestPrice)
                .ThenBy(entry => entry.Item.Name)
                .ThenBy(entry => entry.Item.Id)
        };

        var totalCount = sorted.Count();
        var pageItems = sorted
            .Skip((normalized.Page - 1) * normalized.PageSize)
            .Take(normalized.PageSize)
            .Select(entry => entry.Item)
            .ToList();

        var offersByItem = latestPrices.ToLookup(price => price.ItemId);
        foreach (var item in pageItems)
        {
            item.Prices = offersByItem[item.Id].ToList();
        }

        var catalogItems = pageItems.Select(item => new CatalogItemDto(
            item.Id,
            item.Name,
            item.Prices
                .Where(price => price.Store is not null)
                .OrderBy(price => price.Store!.Name)
                .Select(price => new CatalogOfferDto(
                    price.StoreId,
                    price.Store!.Name,
                    price.Cost,
                    price.RetailCost,
                    price.RecordedAt))
                .ToList()))
            .ToList();

        return new PagedResponse<CatalogItemDto>(catalogItems, totalCount, normalized.Page, normalized.PageSize);
    }

    public async Task<List<StoreDto>> GetStoresAsync()
    {
        var stores = await _storesRepository.GetAllAsync();
        return stores.Select(s => new StoreDto(s.Id, s.Name)).ToList();
    }
}

