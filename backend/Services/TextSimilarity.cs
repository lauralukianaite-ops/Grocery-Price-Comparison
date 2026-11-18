using System.Globalization;
using System.Text;

namespace backend.Services;

public static class TextSimilarity
{
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
}