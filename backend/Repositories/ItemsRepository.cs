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
        var storeFilter = string.IsNullOrWhiteSpace(query.Store)
            ? null
            : query.Store.Trim().ToLower();

        var latestPrices = _context.Prices.AsNoTracking()
            .Where(p => !_context.Prices.Any(n =>
                n.ItemId == p.ItemId &&
                n.StoreId == p.StoreId &&
                (n.RecordedAt > p.RecordedAt ||
                 (n.RecordedAt == p.RecordedAt && n.Id > p.Id))));

        if (storeFilter is not null)
            latestPrices = latestPrices.Where(p => p.Store!.Name.ToLower() == storeFilter);

        if (query.StoreIds is { Count: > 0 })
        {
            latestPrices = latestPrices.Where(price => query.StoreIds.Contains(price.StoreId));
        }

        var perItem = latestPrices
            .GroupBy(p => p.ItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                BestPrice = g.Min(p => p.Cost),
                BestDiscount = g.Max(p =>
                    p.RetailCost != null && p.RetailCost > 0 && p.RetailCost > p.Cost
                        ? (p.RetailCost.Value - p.Cost) / p.RetailCost.Value
                        : 0m)
            });

        var itemsQuery = _context.Items.AsNoTracking().AsQueryable();

        if (query.ItemIds is not null)
            itemsQuery = itemsQuery.Where(i => query.ItemIds.Contains(i.Id));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = TextSimilarity.Normalize(query.Search);
            // Match names without Lithuanian accents before counting results and selecting the page.
            var searchableItems = itemsQuery.Select(item => new { Item = item, Name = item.Name.ToLower() });
            foreach (var letter in "ąčęėįšųūž")
            {
                var accented = letter.ToString();
                var plain = TextSimilarity.Normalize(accented);
                searchableItems = searchableItems.Select(item => new
                {
                    item.Item,
                    Name = item.Name.Replace(accented, plain)
                });
            }
            itemsQuery = searchableItems.Where(item => item.Name.Contains(term)).Select(item => item.Item);
        }

        var joined = itemsQuery.Join(
            perItem,
            item => item.Id,
            stats => stats.ItemId,
            (item, stats) => new
            {
                Item = item,
                stats.BestPrice,
                stats.BestDiscount
            });

        if (query.MinPrice is not null)
        {
            joined = joined.Where(entry => entry.BestPrice >= query.MinPrice.Value);
        }
        if (query.MaxPrice is not null)
        {
            joined = joined.Where(entry => entry.BestPrice <= query.MaxPrice.Value);
        }
        if (query.DiscountedOnly)
        {
            joined = joined.Where(entry => entry.BestDiscount > 0);
        }

        var sorted = query.Sort switch
        {
            CatalogSort.PriceLow => joined.OrderBy(x => x.BestPrice)
                .ThenBy(x => x.Item.Name).ThenBy(x => x.Item.Id),
            CatalogSort.PriceHigh => joined.OrderByDescending(x => x.BestPrice)
                .ThenBy(x => x.Item.Name).ThenBy(x => x.Item.Id),
            CatalogSort.Name => joined.OrderBy(x => x.Item.Name).ThenBy(x => x.Item.Id),
            CatalogSort.NameDescending => joined.OrderByDescending(x => x.Item.Name)
                .ThenBy(x => x.Item.Id),
            CatalogSort.DiscountLow => joined.OrderBy(x => x.BestDiscount).ThenBy(x => x.BestPrice)
                .ThenBy(x => x.Item.Name).ThenBy(x => x.Item.Id),
            _ => joined.OrderByDescending(x => x.BestDiscount).ThenBy(x => x.BestPrice)
                .ThenBy(x => x.Item.Name).ThenBy(x => x.Item.Id)
        };

        var totalCount = await sorted.CountAsync();

        var pageItems = await sorted
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => x.Item)
            .ToListAsync();

        var pageIds = pageItems.Select(i => i.Id).ToList();
        var offers = await latestPrices
            .Where(p => pageIds.Contains(p.ItemId))
            .Include(p => p.Store)
            .ToListAsync();

        var offersByItem = offers.ToLookup(p => p.ItemId);
        foreach (var item in pageItems)
            item.Prices = offersByItem[item.Id].ToList();

        return new CatalogPageResult(pageItems, totalCount);
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
}
