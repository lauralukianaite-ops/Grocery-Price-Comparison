using backend.DTOs;

namespace backend.Services;

public interface IItemSimilarityService
{
    Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, double threshold = 0.3);
}
