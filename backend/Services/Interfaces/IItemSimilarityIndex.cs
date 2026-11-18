using backend.Entities;

namespace backend.Services.Interfaces;

public interface IItemSimilarityIndex
{
    List<SimilarItemMatch> Find(string query, double threshold);
    void BuildOrUpdateIndex(IEnumerable<Item> items);
}