using backend.Entities;

namespace backend.Repositories;

public interface IItemsRepository
{
    Task<List<Item>> GetItemsWithPriceAndStoreAsync();
    Task<bool> ExistsAsync(int itemId);
    Task<List<Price>> GetPriceWithStoreByItemIdAsync(int itemId);
    Task<List<Item>> GetItemsAsync();
    Task<List<Price>> GetNewestPricesWithStoreAndItemSinceAsync(DateTime since);
}