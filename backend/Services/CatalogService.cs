using backend.DTOs;
using backend.Repositories;
using backend.Services.Interfaces;

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
        string? sort,
        string? ids)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        sort = sort is "price-high" or "name" ? sort : "price-low";

        var (items, totalCount) = await _itemsRepository.GetItemsPageAsync(
            page,
            pageSize,
            query,
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
}

