using System.Globalization;
using System.Text;
using backend.DTOs;
using backend.Entities;
using backend.Repositories;
using backend.Services.Interfaces;
using backend.ValueObjects;

namespace backend.Services;

public class ItemSimilarityService : IItemSimilarityService
{
    public static readonly SimilarityScore DefaultThreshold = new(0.3);
    private readonly IItemsRepository _itemsRepository;

    public ItemSimilarityService(IItemsRepository itemsRepository)
    {
        _itemsRepository = itemsRepository;
    }

    public SimilarityScore CalculateSimilarity(string firstName, string secondName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(secondName))
            return SimilarityScore.Zero;

        var normalisedFirst = RemoveDiacritics(firstName);
        var normalisedSecond = RemoveDiacritics(secondName);

        return CalculateTrigramSimilarity(normalisedFirst, normalisedSecond);
    }

    public async Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, SimilarityScore? threshold = null)
    {
        if (string.IsNullOrEmpty(itemName))
            throw new ArgumentException("Item name can't be empty.");

        var allItems = await _itemsRepository.GetItemsWithPriceAndStoreAsync();
        var minimumScore = threshold ?? DefaultThreshold;
        var similarItems = FindSimilarItems(itemName, allItems, minimumScore);

        return new SimilarItemsResponseDto(
            BaseItemName: itemName,
            Threshold: minimumScore.Value,
            SimilarItems: similarItems
        );
    }

    public List<SimilarItemDto> FindSimilarItems(
        string itemName,
        IEnumerable<Item> allItems,
        SimilarityScore? threshold = null)
    {
        var minimumScore = threshold ?? DefaultThreshold;
        var normalizedBaseItemName = RemoveDiacritics(itemName);
        var queryWords = normalizedBaseItemName.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
        var results = new List<SimilarItemDto>();

        foreach (var item in allItems)
        {
            var normalizedItemName = RemoveDiacritics(item.Name);
            var similarity = CalculateTrigramSimilarity(normalizedBaseItemName, normalizedItemName);
            bool containsAnyWord = queryWords.Any(word => word.Length > 2 && normalizedItemName.Contains(word));

            if (similarity >= minimumScore || containsAnyWord)
            {
                var latestPricesPerStore = item.Prices
                .Where(p => p.Store != null)
                .GroupBy(p => p.Store!.Name)
                .Select(g => g.OrderByDescending(p => p.RecordedAt).FirstOrDefault());

                foreach (var latestPrice in latestPricesPerStore)
                {
                    results.Add(new SimilarItemDto(
                        Id: item.Id,
                        Name: item.Name,
                        Store: latestPrice!.Store!.Name,
                        Cost: (double)latestPrice.Cost,
                        RetailCost: latestPrice.RetailCost,
                        SimilarityScore: Math.Round(similarity.Value, 2)
                    ));
                }
            }
        }

        return results.OrderByDescending(x => x.SimilarityScore).ToList();
    }

    private SimilarityScore CalculateTrigramSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return SimilarityScore.Zero;

        str1 = str1.ToLower().Trim();
        str2 = str2.ToLower().Trim();

        var trigrams1 = ExtractTrigrams(str1);
        var trigrams2 = ExtractTrigrams(str2);

        if (trigrams1.Count == 0 || trigrams2.Count == 0)
            return SimilarityScore.Zero;

        int matches = trigrams1.Intersect(trigrams2).Count();

        int union = trigrams1.Union(trigrams2).Count();

        return new SimilarityScore((double)matches / union);
    }
    private HashSet<string> ExtractTrigrams(string str)
    {
        var trigrams = new HashSet<string>();

        if (str.Length < 3)
            return trigrams;

        for (int i = 0; i <= str.Length - 3; i++)
        {
            trigrams.Add(str.Substring(i, 3));
        }

        return trigrams;
    }

    private string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
