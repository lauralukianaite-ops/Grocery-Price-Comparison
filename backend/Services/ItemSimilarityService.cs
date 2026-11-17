using backend.DTOs;
using backend.Repositories;
using backend.Services.Interfaces;

namespace backend.Services;

public class ItemSimilarityService : IItemSimilarityService
{
    public const double DefaultThreshold = 0.3;
    private readonly IItemSimilarityIndex _similarityIndex;
    private readonly IItemsRepository _repository;

    public ItemSimilarityService(IItemSimilarityIndex similarityIndex, IItemsRepository repository)
    {
        _similarityIndex = similarityIndex ?? throw new ArgumentNullException(nameof(similarityIndex));
        _repository = repository;
    }

    public async Task RefreshIndexAsync()
    {
        var items = await _repository.GetItemsWithPriceAndStoreAsync();
        _similarityIndex.BuildOrUpdateIndex(items);
    }

    public Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, double threshold = DefaultThreshold)
    {
        if (threshold < 0 || threshold > 1)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 1.");

        if (string.IsNullOrWhiteSpace(itemName))
            throw new ArgumentException("Item name can't be empty.");

        var matches = _similarityIndex.Find(itemName, threshold);

        var similarItems = matches.SelectMany(ToDtos).ToList();

        var response = new SimilarItemsResponseDto(
            BaseItemName: itemName,
            Threshold: threshold,
            SimilarItems: similarItems
        );

        return Task.FromResult(response);
    }

    public double CalculateSimilarity(string firstName, string secondName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(secondName))
            return 0.0;

        var normalisedFirst = TextSimilarity.Normalize(firstName);
        var normalisedSecond = TextSimilarity.Normalize(secondName);

        var trigramsFirst = TextSimilarity.ExtractTrigrams(normalisedFirst);
        var trigramsSecond = TextSimilarity.ExtractTrigrams(normalisedSecond);

        return TextSimilarity.CalculateJaccard(trigramsFirst,trigramsSecond);
    }

    public static IEnumerable<SimilarItemDto> ToDtos (SimilarItemMatch match)
    {
        var latestPricePerStore = match.Item.Prices
            .Where(p => p.Store != null)
            .GroupBy(p => p.Store!.Name)
            .Select(g => g.OrderByDescending(p => p.RecordedAt).First());
 
        foreach (var price in latestPricePerStore)
        {
            yield return new SimilarItemDto(
                Id: match.Item.Id,
                Name: match.Item.Name,
                Store: price.Store!.Name,
                Cost: (double)price.Cost,
                RetailCost: price.RetailCost,
                SimilarityScore: Math.Round(match.Score, 2)
            );
        }
    }
}