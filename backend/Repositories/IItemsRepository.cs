using backend.Entities;
using backend.ValueObjects;

namespace backend.Repositories;

public interface IItemsRepository
{
    Task<PageResult<Item>> GetCatalogPageAsync(CatalogQuery query);
    Task<List<Item>> GetItemsWithPriceAndStoreAsync();
    Task<bool> ExistsAsync(int itemId);
    Task<List<Price>> GetPriceWithStoreByItemIdAsync(int itemId);
    Task<List<Item>> GetItemsAsync();
    Task<List<Price>> GetNewestPricesWithStoreAndItemSinceAsync(DateTime since, List<int>? bannedIds = null);
}
