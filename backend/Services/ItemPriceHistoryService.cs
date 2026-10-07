using backend.DTOs;
using backend.Repositories;

namespace backend.Services;

public class ItemPriceHistoryService : IItemPriceHistoryService
{
    private readonly IItemsRepository _itemsRepository;
    public ItemPriceHistoryService(IItemsRepository itemsRepository)
    {
        _itemsRepository = itemsRepository;
    }
    public async Task<List<PricePointDto>?> GetPriceHistoryAsync(int itemId)
    {
        var itemsExist = await _itemsRepository.ExistsAsync(itemId);
        if (!itemsExist) return null;
        
        var prices = await _itemsRepository.GetPriceWithStoreByItemIdAsync(itemId);

        return prices
            .OrderBy(p => p.RecordedAt)
            .Select(p => new PricePointDto(
                p.StoreId,
                p.Store!.Name,
                p.Cost,
                p.RetailCost,
                p.RecordedAt
                ))
            .ToList();
    }
}