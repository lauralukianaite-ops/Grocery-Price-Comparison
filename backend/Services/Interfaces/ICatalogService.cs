using backend.DTOs;
using backend.Entities;

namespace backend.Services.Interfaces;

public interface ICatalogService
{
    Task<CatalogResponseDto> GetCatalogAsync(
        int page,
        int pageSize,
        string? query,
        string? store,
        string? sort,
        string? ids);

    Task <List<Store>> GetStoresAsync();
}

