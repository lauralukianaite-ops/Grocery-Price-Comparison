using System.Globalization;
using System.Text;
using backend.DTOs;
using backend.Entities;
using backend.Repositories;
using backend.Services.Interfaces;
using backend.Services;

namespace backend.Services;

public class ItemSimilarityService : IItemSimilarityService
{
    public const double DefaultThreshold = 0.3;
    private readonly IItemSimilarityIndex _similarityIndex;

    public ItemSimilarityService(IItemSimilarityIndex similarityIndex)
    {
        _similarityIndex = similarityIndex;
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