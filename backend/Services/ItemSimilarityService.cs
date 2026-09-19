using backend.DTOs;
using backend.Entities;

namespace backend.Services;

public class ItemSimilarityService
{
    // Finds items similar to the base item using Tri-gram similarity algorithm
    public List<SimilarItemDto> FindSimilarItems(Item baseItem, IEnumerable<Item> allItems, double threshold = 0.3)
    {
        var results = new List<SimilarItemDto>();

        foreach (var item in allItems)
        {
            // Don't compare with itself
            if (item.Id == baseItem.Id)
                continue;

            double similarity = CalculateTrigramSimilarity(baseItem.Name, item.Name);

            // Only include items above threshold
            if (similarity >= threshold)
            {
                results.Add(new SimilarItemDto(
                    Id: item.Id,
                    Name: item.Name,
                    Category: string.Empty,
                    SimilarityScore: Math.Round(similarity, 2)
                ));
            }
        }

        // Sort by similarity score descending
        return results.OrderByDescending(x => x.SimilarityScore).ToList();
    }

    // Calculates similarity between two strings using Tri-gram algorithm
    private double CalculateTrigramSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;

        // Normalize strings: lowercase and remove extra spaces
        str1 = str1.ToLower().Trim();
        str2 = str2.ToLower().Trim();

        // Extract tri-grams
        var trigrams1 = ExtractTrigrams(str1);
        var trigrams2 = ExtractTrigrams(str2);

        if (trigrams1.Count == 0 || trigrams2.Count == 0)
            return 0.0;

        // Count matching tri-grams
        int matches = trigrams1.Intersect(trigrams2).Count();

        // Calculate Jaccard similarity: intersection / union
        int union = trigrams1.Union(trigrams2).Count();

        return (double)matches / union;
    }

    // Extracts all tri-grams (3-character sequences) from a string
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
}
