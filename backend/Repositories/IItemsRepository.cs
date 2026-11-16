using backend.Entities;

namespace backend.Repositories;

public interface IItemsRepository
{
    Task<(List<Item> Items, int TotalCount)> GetItemsPageAsync(
        int page,
        int pageSize,
        string? query,
        string sort,
        List<int>? itemIds = null);
    Task<List<Item>> GetItemsWithPriceAndStoreAsync();
    Task<bool> ExistsAsync(int itemId);
    Task<List<Price>> GetPriceWithStoreByItemIdAsync(int itemId);
    Task<List<Item>> GetItemsAsync();
    Task<List<Price>> GetNewestPricesWithStoreAndItemSinceAsync(DateTime since, List<int>? bannedIds = null);
}
