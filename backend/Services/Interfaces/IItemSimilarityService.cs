using backend.DTOs;
using backend.Entities;
using backend.ValueObjects;

namespace backend.Services.Interfaces;

public interface IItemSimilarityService
{
    Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, SimilarityScore? threshold = null);
    SimilarityScore CalculateSimilarity(string firstName, string secondName);
    
}
