using System.Globalization;
using System.Text;
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
            var normalized = Normalize(item.Name);
            var trigrams = ExtractTrigrams(normalized);
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
        var normalizedQuery = Normalize(query);

        if(string.IsNullOrEmpty(normalizedQuery) || currentItems.Count == 0)
            return new List<SimilarItemDto>();

        var queryTrigrams = ExtractTrigrams(normalizedQuery);
        var queryWords = normalizedQuery.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length > 2).ToArray();

        var results = new List<SimilarItemDto>();

        foreach(var precomputed in currentItems)
        {
            bool containsWord = queryWords.Any(w => precomputed.NormalizedName.Contains(w, StringComparison.Ordinal));
            double similarity = CalculateJaccard(queryTrigrams, precomputed.Trigrams);

            if((similarity>0 && similarity >= threshold) || containsWord)
            {
                AddLatestPricesPerStore(results,precomputed.Item,similarity);
            }
        }
        return results.OrderByDescending(x => x.SimilarityScore).ToList();
    }

    public static double CalculateJaccard(HashSet<string> set1, HashSet<string> set2)
    {
        if(set1.Count==0 || set2.Count==0)
            return 0.0;

        int matches = 0;
        foreach(var trigram in set1)
        {
            if(set2.Contains(trigram))
                matches++;
        }

        int union = set1.Count + set2.Count - matches;
        return (double)matches/union;
    }

    public static HashSet<string> ExtractTrigrams(string text)
    {
        var trigrams = new HashSet<string>();

        if(text.Length<3) 
            return trigrams;

        for(int i=0; i<=text.Length - 3; i++)
        {
            trigrams.Add(text.Substring(i,3));
        }
        return trigrams;
    }

    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var decomposed = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();
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