using backend.DTOs;
using backend.Entities;
using backend.Services.Interfaces;

namespace backend.Services;

internal record PrecomputedItem(
    Item Item,
    string NormalizedName,
    HashSet<string> Trigrams
);

public class ItemSimilarityIndex : IItemSimilarityIndex
{
    private static readonly char[] Separators = { ' ', ',', '-' };

    private volatile List<PrecomputedItem> _indexedItems = new();
    private readonly object _syncLock = new();

    public void BuildOrUpdateIndex(IEnumerable<Item> items)
    {
        var freshList = items.Select(item=>
        {
            var normalized = TextSimilarity.Normalize(item.Name);
            var trigrams = TextSimilarity.ExtractTrigrams(normalized);
            return new PrecomputedItem(item,normalized,trigrams);
        }).ToList();

        lock (_syncLock)
        {
            _indexedItems = freshList;
        }
    }

    public List<SimilarItemDto> Find(string query, double threshold = ItemSimilarityService.DefaultThreshold)
    {
        var currentItems = _indexedItems;
        var normalizedQuery = TextSimilarity.Normalize(query);

        if(string.IsNullOrEmpty(normalizedQuery) || currentItems.Count == 0)
            return new List<SimilarItemDto>();

        var queryTrigrams = TextSimilarity.ExtractTrigrams(normalizedQuery);
        var queryWords = normalizedQuery.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length > 2).ToArray();

        var results = new List<SimilarItemDto>();

        foreach(var precomputed in currentItems)
        {
            bool containsWord = queryWords.Any(w => precomputed.NormalizedName.Contains(w, StringComparison.Ordinal));
            double similarity = TextSimilarity.CalculateJaccard(queryTrigrams, precomputed.Trigrams);

            if((similarity>0 && similarity >= threshold) || containsWord)
            {
                AddLatestPricesPerStore(results,precomputed.Item,similarity);
            }
        }
        return results.OrderByDescending(x => x.SimilarityScore).ToList();
    }

    private static void AddLatestPricesPerStore(List<SimilarItemDto> results, Item item, double similarity)
    {
        var latestPricesPerStore = item.Prices
            .Where(p => p.Store != null)
            .GroupBy(p => p.Store!.Name)
            .Select(g => g.OrderByDescending(p => p.RecordedAt).First());

        foreach (var latestPrice in latestPricesPerStore)
        {
            results.Add(new SimilarItemDto(
                Id: item.Id,
                Name: item.Name,
                Store: latestPrice.Store!.Name,
                Cost: (double)latestPrice.Cost,
                RetailCost: latestPrice.RetailCost,
                SimilarityScore: Math.Round(similarity, 2)
            ));
        }
    }

};