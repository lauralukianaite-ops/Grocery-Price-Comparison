using System.Collections.Generic;
using backend.Services;
using Xunit;

namespace backend.Tests;

public class TextSimilarityTests
{
    [Fact]
    public void Normalize_RemovesDiacritics_AndLowercases()
    {
        Assert.Equal("sviezia varske 9%", TextSimilarity.Normalize("Šviežia varškė 9%"));
    }

    [Fact]
    public void Normalize_ReturnsEmpty_ForBlankInput()
    {
        Assert.Equal(string.Empty, TextSimilarity.Normalize("   "));
    }

    [Fact]
    public void ExtractTrigrams_ReturnsEmpty_ForShortText()
    {
        Assert.Empty(TextSimilarity.ExtractTrigrams("ab"));
    }

    [Fact]
    public void ExtractTrigrams_ReturnsAllThreeLetterSubstrings()
    {
        var trigrams = TextSimilarity.ExtractTrigrams("pienas");

        Assert.Equal(new HashSet<string> { "pie", "ien", "ena", "nas" }, trigrams);
    }

    [Fact]
    public void CalculateJaccard_ReturnsExpectedValues()
    {
        var a = new HashSet<string> { "abc", "bcd" };
        var b = new HashSet<string> { "bcd", "cde" };

        Assert.Equal(1.0 / 3.0, TextSimilarity.CalculateJaccard(a, b), precision: 5);
        Assert.Equal(0.0, TextSimilarity.CalculateJaccard(a, new HashSet<string>()));
    }
}