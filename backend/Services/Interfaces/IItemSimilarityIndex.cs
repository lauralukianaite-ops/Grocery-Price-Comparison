using backend.DTOs;
using backend.Entities;

namespace backend.Services.Interfaces;

public interface IItemSimilarityIndex
{
    List<SimilarItemDto> Find(string query, double threshold = ItemSimilarityService.DefaultThreshold);
    void BuildOrUpdateIndex(IEnumerable<Item> items);
}