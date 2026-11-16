using System.Collections.Generic;
using System.Linq;
using backend.Entities;
using backend.Services;
using Xunit;

namespace backend.Tests;

public class ItemSimilarityIndexTests
{
    private static ItemSimilarityIndex CreateIndex(params (int id, string name)[] items)
    {
        var index = new ItemSimilarityIndex();
        index.BuildOrUpdateIndex(items.Select(i => new Item { Id = i.id, Name = i.name }).ToList());
        return index;
    }

    [Fact]
    public void Find_ReturnsMilkVariants_AndExcludesBread()
    {
        var index = CreateIndex(
            (2, "Pienas 2.5% (500ml)"),
            (3, "Skystas Pienas 2.5% 1L"),
            (4, "Duona šviesi"),
            (5, "Pienas 3.5% 1l"));

        var results = index.Find("Pienas", 0.1);

        Assert.Contains(results, r => r.Item.Id == 2);
        Assert.Contains(results, r => r.Item.Id == 3);
        Assert.Contains(results, r => r.Item.Id == 5);
        Assert.DoesNotContain(results, r => r.Item.Id == 4);
    }

    [Fact]
    public void Find_WithHighThreshold_ExcludesUnrelatedItems()
    {
        var index = CreateIndex(
            (1, "Prancūziškas česnakinis batonas"),
            (2, "Batonas prancūziškas česnakinis"),
            (3, "Samsung ultra hd max pro phone"));

        var results = index.Find("Prancuziskas cesnakinis batonas", 0.8);

        Assert.Contains(results, r => r.Item.Id == 1);
        Assert.Contains(results, r => r.Item.Id == 2);
        Assert.DoesNotContain(results, r => r.Item.Id == 3);
    }

    [Fact]
    public void Find_ReturnsBestMatchFirst()
    {
        var index = CreateIndex(
            (1, "Pienas 2.5% 1l dvaro"),
            (2, "Pienas"));

        var results = index.Find("Pienas", 0.1);

        Assert.Equal(2, results.First().Item.Id);
        Assert.Equal(results.OrderByDescending(r => r.Score).Select(r => r.Item.Id),
                     results.Select(r => r.Item.Id));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Find_ReturnsEmpty_WhenQueryIsBlank(string query)
    {
        var index = CreateIndex((1, "Ananasas"));

        Assert.Empty(index.Find(query, 0.4));
    }

    [Fact]
    public void Find_ReturnsEmpty_WhenIndexIsEmpty()
    {
        var index = new ItemSimilarityIndex();

        Assert.Empty(index.Find("Pienas", 0.1));
    }

    [Fact]
    public void BuildOrUpdateIndex_ReplacesPreviousContent()
    {
        var index = CreateIndex((1, "Pienas"));

        index.BuildOrUpdateIndex(new List<Item> { new Item { Id = 2, Name = "Duona" } });

        Assert.Empty(index.Find("Pienas", 0.1));
        Assert.Single(index.Find("Duona", 0.1));
    }
}