using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class ItemsRepository : IItemsRepository
{
    private readonly AppDbContext _context;

    public ItemsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Item> Items, int TotalCount)> GetItemsPageAsync(
        int page,
        int pageSize,
        string? query,
        string sort,
        List<int>? itemIds = null)
    {
        var itemsQuery = _context.Items.AsNoTracking();

        if (itemIds is not null)
            itemsQuery = itemsQuery.Where(item => itemIds.Contains(item.Id));

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.Trim().ToLower();
            itemsQuery = itemsQuery.Where(item => item.Name.ToLower().Contains(searchTerm));
        }

        var items = await itemsQuery
            .Include(item => item.Prices)
            .ThenInclude(price => price.Store)
            .ToListAsync();

        var sortedItems = sort switch
        {
            "price-high" => items.OrderBy(item => GetBestCurrentPrice(item) is null)
                .ThenByDescending(GetBestCurrentPrice).ThenBy(item => item.Name).ThenBy(item => item.Id),
            "name" => items.OrderBy(item => item.Name).ThenBy(item => item.Id),
            _ => items.OrderBy(item => GetBestCurrentPrice(item) is null)
                .ThenBy(GetBestCurrentPrice).ThenBy(item => item.Name).ThenBy(item => item.Id)
        };

        return (sortedItems.Skip((page - 1) * pageSize).Take(pageSize).ToList(), items.Count);
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
}
