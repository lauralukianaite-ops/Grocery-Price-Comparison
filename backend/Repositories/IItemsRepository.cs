using backend.Entities;

namespace backend.Repositories;

public interface IItemsRepository
{
    Task<List<Price>> GetLatestPricesWithStoreAsync(string? storeName = null, List<int>? storeIds = null);
    Task<List<Item>> GetItemsWithPriceAndStoreAsync();
    Task<bool> ExistsAsync(int itemId);
    Task<List<Price>> GetPriceWithStoreByItemIdAsync(int itemId);
    Task<List<Item>> GetItemsAsync();
    Task<List<Price>> GetNewestPricesWithStoreAndItemSinceAsync(DateTime since, List<int>? bannedIds = null);
}
