using backend.DTOs;
using backend.ValueObjects;
using backend.Entities;

namespace backend.Services.Interfaces;

public interface ICatalogService
{
    Task<CatalogResponseDto> GetCatalogAsync(CatalogQuery query);

    Task <List<Store>> GetStoresAsync();
}

