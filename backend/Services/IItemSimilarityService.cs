using backend.DTOs;
using backend.Entities;

namespace backend.Services;

public interface IItemSimilarityService
{
    Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, double threshold = 0.2);
    double CalculateSimilarity(string firstName, string secondName);
    
}
