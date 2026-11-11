using backend.DTOs;
using backend.Repositories;
using backend.Services.Interfaces;

namespace backend.Services;

public class ItemSimilarityService : IItemSimilarityService
{
    public const double DefaultThreshold = 0.3;
    private readonly IItemSimilarityIndex _similarityIndex;
    private readonly IItemsRepository _repository;

    public ItemSimilarityService(IItemSimilarityIndex similarityIndex, IItemsRepository? repository = null)
    {
        _similarityIndex = similarityIndex;
        _repository = repository ?? throw new InvalidOperationException("Repository is not configured for this service instance.");;
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

        if (string.IsNullOrEmpty(itemName))
            throw new ArgumentException("Item name can't be empty.");

        var similarItems = _similarityIndex.Find(itemName, threshold);

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

        var normalisedFirst = ItemSimilarityIndex.Normalize(firstName);
        var normalisedSecond = ItemSimilarityIndex.Normalize(secondName);

        var trigramsFirst = ItemSimilarityIndex.ExtractTrigrams(normalisedFirst);
        var trigramsSecond = ItemSimilarityIndex.ExtractTrigrams(normalisedSecond);

        return ItemSimilarityIndex.CalculateJaccard(trigramsFirst,trigramsSecond);
    }
}