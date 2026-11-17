using backend.Data;
using backend.Entities;
using backend.ValueObjects;
using backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class ItemsRepository : IItemsRepository
{
    private readonly AppDbContext _context;

    public ItemsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CatalogPageResult> GetCatalogPageAsync(CatalogQuery query)
    {
        var itemsQuery = _context.Items.AsNoTracking();

        if (query.ItemIds is not null)
            itemsQuery = itemsQuery.Where(item => query.ItemIds.Contains(item.Id));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.Trim().ToLower();
            itemsQuery = itemsQuery.Where(item => item.Name.ToLower().Contains(searchTerm));
        }

        IQueryable<Item> itemsWithIncludes;
        if (!string.IsNullOrWhiteSpace(query.Store))
        {
            var storeFilter = query.Store.Trim().ToLower();
            itemsQuery = itemsQuery.Where(item =>
                item.Prices.Any(p => p.Store != null && p.Store.Name.ToLower() == storeFilter));

            itemsWithIncludes = itemsQuery
                .Include(item => item.Prices.Where(p => p.Store != null && p.Store.Name.ToLower() == storeFilter))
                .ThenInclude(price => price.Store);
        }
        else
        {
            itemsWithIncludes = itemsQuery
                .Include(item => item.Prices)
                .ThenInclude(price => price.Store);
        }

        var items = await itemsWithIncludes.ToListAsync();

        var sortedItems = query.Sort switch
        {
            CatalogSort.DiscountHigh => items
                .OrderByDescending(GetBestCurrentDiscountPercentage)
                .ThenBy(item => GetBestCurrentPrice(item) is null)
                .ThenBy(GetBestCurrentPrice)
                .ThenBy(item => item.Name)
                .ThenBy(item => item.Id),
            CatalogSort.PriceHigh => items.OrderBy(item => GetBestCurrentPrice(item) is null)
                .ThenByDescending(GetBestCurrentPrice).ThenBy(item => item.Name).ThenBy(item => item.Id),
            CatalogSort.Name => items.OrderBy(item => item.Name).ThenBy(item => item.Id),
            _ => items.OrderBy(item => GetBestCurrentPrice(item) is null)
                .ThenBy(GetBestCurrentPrice).ThenBy(item => item.Name).ThenBy(item => item.Id)
        };

        var page = sortedItems.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();
        return new CatalogPageResult(page, items.Count);
    }

    private static decimal GetBestCurrentDiscountPercentage(Item item)
    {
        var latestOffers = item.Prices
            .GroupBy(price => price.StoreId)
            .Select(group => group.OrderByDescending(price => price.RecordedAt).First());
        
        decimal maxDiscount = 0m;
        foreach (var offer in latestOffers)
        {
            if(offer.RetailCost is > 0 && offer.RetailCost > offer.Cost)
            {
                var discount = (offer.RetailCost.Value - offer.Cost) / offer.RetailCost.Value;
                if(discount > maxDiscount)
                    maxDiscount = discount;
            }
        }
        return maxDiscount;
    }

    private static decimal? GetBestCurrentPrice(Item item)
    {
        return item.Prices
            .GroupBy(price => price.StoreId)
            .Select(group => (decimal?)group.OrderByDescending(price => price.RecordedAt).First().Cost)
            .Min();
    }

    public async Task<List<Item>> GetItemsWithPriceAndStoreAsync()
    {
        return await _context.Items
            .Include(i => i.Prices)
            .ThenInclude(p => p.Store)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int itemId)
    {
        return await _context.Items.AnyAsync(i => i.Id == itemId);
    }

    public async Task<List<Price>> GetPriceWithStoreByItemIdAsync(int itemId)
    {
        return await _context.Prices
            .Where(p => p.ItemId == itemId)
            .Include(p => p.Store)
            .ToListAsync();
    }

    public async Task<List<Item>> GetItemsAsync()
    {
        return await _context.Items.ToListAsync();
    }

    public async Task<List<Price>> GetNewestPricesWithStoreAndItemSinceAsync(DateTime since,
        List<int>? bannedIds = null)
    {
        var query = _context.Prices.Where(p => p.RecordedAt >= since);

        if (bannedIds != null)
            query = query.Where(p => !bannedIds.Contains(p.ItemId));

        var freshPrices = await query
            .Include(p => p.Store)
            .Include(p => p.Item)
            .ToListAsync();

        return freshPrices
            .GroupBy(p => new { p.ItemId, p.StoreId })
            .Select(g => g.OrderByDescending(p => p.RecordedAt).First())
            .ToList();
    }

    public async Task<List<Store>> GetStoresAsync()
    {
        return await _context.Stores
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
}
