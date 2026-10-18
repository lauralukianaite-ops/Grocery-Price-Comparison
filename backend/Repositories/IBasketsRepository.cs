using backend.Entities;

namespace backend.Repositories;

public interface IBasketsRepository
{
    Task SaveAsync(Basket basket);
    Task<Basket?> GetByIdAsync(Guid id);
}
