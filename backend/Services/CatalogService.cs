using backend.DTOs;
using backend.Repositories;
using backend.ValueObjects;
using backend.Services.Interfaces;
using backend.Entities;

namespace backend.Services;

public class CatalogService : ICatalogService
{
    private readonly IItemsRepository _itemsRepository;
    private readonly IStoresRepository _storesRepository;

    public CatalogService(IItemsRepository itemsRepository, IStoresRepository storesRepository)
    {
        _itemsRepository = itemsRepository;
        _storesRepository = storesRepository;
    }

    public async Task<CatalogResponseDto> GetCatalogAsync(CatalogQuery query)
    {
        var normalized = query with
        {
            Page = Math.Max(query.Page, 1),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };

        var result = await _itemsRepository.GetCatalogPageAsync(normalized);

        var catalogItems = result.Items.Select(item => new CatalogItemDto(
            item.Id,
            item.Name,
            item.Prices
            .Where(price => price.Store is not null)
                .GroupBy(price => price.StoreId)
                .Select(group => group.OrderByDescending(price => price.RecordedAt).First())
                .OrderBy(price => price.Store!.Name)
                .Select(price => new CatalogOfferDto(
                    price.StoreId,
                    price.Store!.Name,
                    price.Cost,
                    price.RetailCost,
                    price.RecordedAt))
                .ToList()))
            .ToList();

        return new CatalogResponseDto(catalogItems, result.TotalCount, normalized.Page, normalized.PageSize);
    }

    public async Task<List<StoreDto>> GetStoresAsync()
    {
        var stores = await _storesRepository.GetAllAsync();
        return stores.Select(s => new StoreDto(s.Id, s.Name)).ToList();
    }
}

