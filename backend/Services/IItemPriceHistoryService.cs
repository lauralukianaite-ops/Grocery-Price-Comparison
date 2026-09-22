using backend.DTOs;
using backend.Entities;

namespace backend.Services;

public interface IItemPriceHistoryService
{
    Task<List<PricePointDto>?> GetPriceHistoryAsync(int itemId);
}