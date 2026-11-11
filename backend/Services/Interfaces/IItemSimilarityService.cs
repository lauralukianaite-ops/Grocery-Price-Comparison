using backend.DTOs;
using backend.Entities;

namespace backend.Services.Interfaces;

public interface IItemSimilarityService
{
    Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, double threshold = ItemSimilarityService.DefaultThreshold);
    double CalculateSimilarity(string firstName, string secondName);
    
}
