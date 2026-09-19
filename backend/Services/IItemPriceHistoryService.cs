using backend.Entities;

namespace backend.Services;

public interface IItemPriceHistoryService
{
    Task<IEnumerable<Item>> GetPriceHistoryAsync(int  itemId);
}