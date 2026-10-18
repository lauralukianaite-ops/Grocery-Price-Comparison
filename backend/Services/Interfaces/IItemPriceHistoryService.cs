using backend.DTOs;

namespace backend.Services.Interfaces;

public interface IItemPriceHistoryService
{
    Task<List<PricePointDto>?> GetPriceHistoryAsync(int itemId);
}