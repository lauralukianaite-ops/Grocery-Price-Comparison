using backend.DTOs;

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
}

