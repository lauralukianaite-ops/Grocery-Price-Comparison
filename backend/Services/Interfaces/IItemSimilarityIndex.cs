using backend.DTOs;
using backend.Entities;

namespace backend.Services.Interfaces;

public record SimilarItemMatch(Item Item, double Score);

public interface IItemSimilarityIndex
{
    List<SimilarItemMatch> Find(string query, double threshold);
    void BuildOrUpdateIndex(IEnumerable<Item> items);
}