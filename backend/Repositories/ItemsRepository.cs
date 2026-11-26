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

    public async Task<List<Price>> GetLatestPricesWithStoreAsync(string? storeName = null, List<int>? storeIds = null)
    {
        var latestPrices = _context.Prices.AsNoTracking()
            .Where(p => !_context.Prices.Any(n =>
                n.ItemId == p.ItemId &&
                n.StoreId == p.StoreId &&
                (n.RecordedAt > p.RecordedAt ||
                 (n.RecordedAt == p.RecordedAt && n.Id > p.Id))));

        if (!string.IsNullOrWhiteSpace(storeName))
            latestPrices = latestPrices.Where(p => p.Store!.Name.ToLower() == storeName);

        if (storeIds is { Count: > 0 })
        {
            latestPrices = latestPrices.Where(price => storeIds.Contains(price.StoreId));
        }

        return await latestPrices
            .Include(p => p.Store)
            .Include(p => p.Item)
            .ToListAsync();
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
