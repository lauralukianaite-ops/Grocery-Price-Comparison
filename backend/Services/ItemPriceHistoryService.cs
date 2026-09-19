using backend.DTOs;
using backend.Entities;

namespace backend.Services;

public class ItemPriceHistoryService : IItemPriceHistoryService
{
    public async Task<List<PricePointDto>?> GetPriceHistoryAsync(int itemId)
    {
        throw new NotImplementedException();
    }
}