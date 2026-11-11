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

    public List<SimilarItemMatch> Find(string query, double threshold)
    {
        var currentItems = _indexedItems;
        var normalizedQuery = TextSimilarity.Normalize(query);

        if(string.IsNullOrEmpty(normalizedQuery) || currentItems.Count == 0)
            return new List<SimilarItemMatch>();

        var queryTrigrams = TextSimilarity.ExtractTrigrams(normalizedQuery);
        var queryWords = normalizedQuery.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length > 2).ToArray();

        var results = new List<SimilarItemMatch>();

        foreach(var precomputed in currentItems)
        {
            bool containsWord = queryWords.Any(w => precomputed.NormalizedName.Contains(w, StringComparison.Ordinal));
            double similarity = TextSimilarity.CalculateJaccard(queryTrigrams, precomputed.Trigrams);

            if((similarity>0 && similarity >= threshold) || containsWord)
            {
                results.Add(new SimilarItemMatch(precomputed.Item,similarity));
            }
        }
        return results.OrderByDescending(x => x.Score).ToList();
    }
};