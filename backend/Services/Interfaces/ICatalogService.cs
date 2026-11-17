using backend.DTOs;
using backend.ValueObjects;

namespace backend.Services.Interfaces;

public interface ICatalogService
{
    Task<CatalogResponseDto> GetCatalogAsync(CatalogQuery query);

    Task<List<StoreDto>> GetStoresAsync();
}

