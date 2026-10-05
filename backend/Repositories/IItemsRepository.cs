using backend.Entities;

namespace backend.Repositories;

public interface IItemsRepository
{
    Task<List<Item>> GetItemsWithStoreAsync();
}