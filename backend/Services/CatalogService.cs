using backend.DTOs;
using backend.Repositories;
using backend.ValueObjects;
using backend.Services.Interfaces;
using backend.Entities;

namespace backend.Services;

public class CatalogService : ICatalogService
{
    private readonly IItemsRepository _itemsRepository;

    public CatalogService(IItemsRepository itemsRepository)
    {
        _itemsRepository = itemsRepository;
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

    public async Task<List<Store>> GetStoresAsync()
    {
        return await _itemsRepository.GetStoresAsync();
    }
}

