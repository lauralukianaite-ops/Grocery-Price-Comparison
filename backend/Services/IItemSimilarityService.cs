using backend.DTOs;

namespace backend.Services;

public interface IItemSimilarityService
{
    Task<SimilarItemsResponseDto> GetSimilarItemsAsync(int itemId, double threshold = 0.4);
}
