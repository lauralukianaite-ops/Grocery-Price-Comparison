using System.Globalization;
using System.Text;
using backend.DTOs;
using backend.Entities;
using backend.Repositories;
using backend.Services.Interfaces;
<<<<<<< HEAD
using backend.ValueObjects;
=======
using backend.Services;
>>>>>>> 89db8c44eaf7df1e7cc435191447c148da7cc258

namespace backend.Services;

public class ItemSimilarityService : IItemSimilarityService
{
<<<<<<< HEAD
    public static readonly SimilarityScore DefaultThreshold = new(0.3);
    private readonly IItemsRepository _itemsRepository;
=======
    public const double DefaultThreshold = 0.3;
    private readonly IItemSimilarityIndex _similarityIndex;
>>>>>>> 89db8c44eaf7df1e7cc435191447c148da7cc258

    public ItemSimilarityService(IItemSimilarityIndex similarityIndex)
    {
        _similarityIndex = similarityIndex;
    }

<<<<<<< HEAD
    public SimilarityScore CalculateSimilarity(string firstName, string secondName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(secondName))
            return SimilarityScore.Zero;

        var normalisedFirst = RemoveDiacritics(firstName);
        var normalisedSecond = RemoveDiacritics(secondName);

        return CalculateTrigramSimilarity(normalisedFirst, normalisedSecond);
    }

    public async Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, SimilarityScore? threshold = null)
=======
    public Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, double threshold = DefaultThreshold)
>>>>>>> 89db8c44eaf7df1e7cc435191447c148da7cc258
    {
        if (string.IsNullOrEmpty(itemName))
            throw new ArgumentException("Item name can't be empty.");

<<<<<<< HEAD
        var allItems = await _itemsRepository.GetItemsWithPriceAndStoreAsync();
        var minimumScore = threshold ?? DefaultThreshold;
        var similarItems = FindSimilarItems(itemName, allItems, minimumScore);

        return new SimilarItemsResponseDto(
=======
        var similarItems = _similarityIndex.Find(itemName, threshold);

        var response = new SimilarItemsResponseDto(
>>>>>>> 89db8c44eaf7df1e7cc435191447c148da7cc258
            BaseItemName: itemName,
            Threshold: minimumScore.Value,
            SimilarItems: similarItems
        );

        return Task.FromResult(response);
    }

<<<<<<< HEAD
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
=======
    public double CalculateSimilarity(string firstName, string secondName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(secondName))
            return 0.0;
>>>>>>> 89db8c44eaf7df1e7cc435191447c148da7cc258

        var normalisedFirst = ItemSimilarityIndex.Normalize(firstName);
        var normalisedSecond = ItemSimilarityIndex.Normalize(secondName);

        var trigramsFirst = ItemSimilarityIndex.ExtractTrigrams(normalisedFirst);
        var trigramsSecond = ItemSimilarityIndex.ExtractTrigrams(normalisedSecond);

<<<<<<< HEAD
        if (trigrams1.Count == 0 || trigrams2.Count == 0)
            return SimilarityScore.Zero;

        int matches = trigrams1.Intersect(trigrams2).Count();

        int union = trigrams1.Union(trigrams2).Count();

        return new SimilarityScore((double)matches / union);
=======
        return ItemSimilarityIndex.CalculateJaccard(trigramsFirst,trigramsSecond);
>>>>>>> 89db8c44eaf7df1e7cc435191447c148da7cc258
    }
}