using backend.Entities;

namespace backend.Repositories;

public interface IStoresRepository
{
    Task<List<Store>> GetAllAsync();
}