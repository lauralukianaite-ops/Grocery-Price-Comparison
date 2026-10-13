using backend.DTOs;

namespace backend.Services.Interfaces;

public interface IItemSimilarityService
{
    Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, double threshold = 0.3);
}
