using backend.Data;
using backend.DTOs;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ItemPriceHistoryService : IItemPriceHistoryService
{
    private readonly AppDbContext _dbContext;
    public ItemPriceHistoryService(AppDbContext db)
    {
        _dbContext = db;
    }
    public async Task<List<PricePointDto>?> GetPriceHistoryAsync(int itemId)
    {
        var itemsExist = await _dbContext.Items.AnyAsync(i => i.Id == itemId);
        if (!itemsExist) return null;
        
        return await _dbContext.Prices
            .Where(p => p.ItemId == itemId)
            .OrderBy(p => p.RecordedAt)
            .Select(p => new PricePointDto(
                p.StoreId,
                p.Store.Name,
                p.Amount,
                p.RecordedAt
                ))
            .ToListAsync();
    }
}