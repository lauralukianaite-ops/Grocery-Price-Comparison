using backend.DTOs;
using backend.Repositories;
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

    public async Task<CatalogResponseDto> GetCatalogAsync(
        int page,
        int pageSize,
        string? query,
        string? store,
        string? sort,
        string? ids)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        sort = sort is "price-low" or "price-high" or "name" or "discount-high" ? sort : "discount-high";

        var (items, totalCount) = await _itemsRepository.GetItemsPageAsync(
            page,
            pageSize,
            query,
            store,
            sort,
            ParseItemIds(ids));

        var catalogItems = items.Select(item => new CatalogItemDto(
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

        return new CatalogResponseDto(catalogItems, totalCount, page, pageSize);
    }

    private static List<int>? ParseItemIds(string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids)) return null;

        return ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var itemId) ? itemId : (int?)null)
            .OfType<int>()
            .Distinct()
            .ToList();
    }

    public async Task<List<Store>> GetStoresAsync()
    {
        return await _itemsRepository.GetStoresAsync();
    }
}

