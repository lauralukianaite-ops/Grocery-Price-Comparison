using backend.DTOs;
using backend.Entities;

namespace backend.Services;

public interface IItemSimilarityService
{
    Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, double threshold = 0.2, bool strictMode = false);
    List<SimilarItemDto> FindSimilarItems(string itemName, IEnumerable<Item> allItems, double threshold = 0.4, bool strictMode = false);
}
