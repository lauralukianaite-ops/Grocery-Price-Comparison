using backend.Data;
using backend.DTOs;
using backend.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services;

public class ItemSimilarityService : IItemSimilarityService
{
    private readonly AppDbContext _context;

    public ItemSimilarityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SimilarItemsResponseDto> GetSimilarItemsAsync(string itemName, double threshold)
    {
        if (threshold < 0 || threshold > 1)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 1.");

        if (string.IsNullOrEmpty(itemName))
            throw new ArgumentException("Item name can't be empty.");

        var allItems = await _context.Items
            .Include(i => i.Prices)
            .ThenInclude(p => p.Store)
            .ToListAsync();
        var similarItems = FindSimilarItems(itemName, allItems, threshold);

        return new SimilarItemsResponseDto
        {
            BaseItemName = itemName,
            Threshold = threshold,
            SimilarItems = similarItems
        };
    }

    public List<SimilarItemDto> FindSimilarItems(string itemName, IEnumerable<Item> allItems, double threshold = 0.3)
    {
        var normalizedBaseItemName = RemoveDiacritics(itemName);
        var queryWords = normalizedBaseItemName.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
        var results = new List<SimilarItemDto>();

        foreach (var item in allItems)
        {
            var normalizedItemName = RemoveDiacritics(item.Name);
            double similarity = CalculateTrigramSimilarity(normalizedBaseItemName, normalizedItemName);
            bool containsAnyWord = queryWords.Any(word => word.Length > 2 && normalizedItemName.Contains(word));

            if (similarity >= threshold || containsAnyWord)
            {
                var latestPricesPerStore = item.Prices
                .Where(p => p.Store != null)
                .GroupBy(p => p.Store.Name)
                .Select(g => g.OrderByDescending(p => p.RecordedAt).FirstOrDefault());

                foreach (var latestPrice in latestPricesPerStore)
                {
                    results.Add(new SimilarItemDto(
                        Id: item.Id,
                        Name: item.Name,
                        Store: latestPrice.Store.Name,
                        Price: (double)latestPrice.Amount,
                        SimilarityScore: Math.Round(similarity, 2)
                    ));
                }
            }
        }

        return results.OrderByDescending(x => x.SimilarityScore).ToList();
    }

    private double CalculateTrigramSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;

        str1 = str1.ToLower().Trim();
        str2 = str2.ToLower().Trim();

        var trigrams1 = ExtractTrigrams(str1);
        var trigrams2 = ExtractTrigrams(str2);

        if (trigrams1.Count == 0 || trigrams2.Count == 0)
            return 0.0;

        int matches = trigrams1.Intersect(trigrams2).Count();

        int union = trigrams1.Union(trigrams2).Count();

        return (double)matches / union;
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
