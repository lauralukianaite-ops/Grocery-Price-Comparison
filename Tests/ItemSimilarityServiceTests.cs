using System;
using System.Collections.Generic;
using backend.Entities;
using backend.Services;
using Superpower.Model;
using Xunit;

namespace backend.Tests;

public class ItemSimilarityServiceTests
{
    [Fact]
    public void FindSimilarItems_ReturnsExpectedMilkVariants()
    {
        var items = new List<Item>
        {
            new Item { Id = 2, Name = "Pienas 2.5% (500ml)" },
            new Item { Id = 3, Name = "Skystas Pienas 2.5% 1L" },
            new Item { Id = 4, Name = "Duona šviesi" },
            new Item { Id = 5, Name = "Pienas 3.5% 1l" }
        };

        var results = new ItemSimilarityService(null).FindSimilarItems("Pieno", items, 0.1);

        Assert.Contains(results, r => r.Id == 2);
        Assert.Contains(results, r => r.Id == 3);
        Assert.DoesNotContain(results, r => r.Id == 4);
        Assert.Contains(results, r => r.Id == 5);
        Assert.True(results.Count >= 2);
    }

    [Fact]
    public void FindSimilarItems_WithHighTreshold()
    {
        var items = new List<Item>
        {
            new Item { Id = 1, Name = "Prancūziškas česnakinis batonas"},
            new Item { Id = 2, Name = "Batonas prancūziškas česnakinis"},
            new Item { Id = 3, Name = "Samsung ultra hd max pro phone"}
        };

        var results = new ItemSimilarityService(null).FindSimilarItems("Prancūziškas česnakinis batonas", items, 0.8);

        Assert.Contains(results, r => r.Id == 1);
        Assert.Contains(results, r => r.Id == 2);
        Assert.DoesNotContain(results, r => r.Id == 3);
    }

    [Fact]
    public void FindSimilarItems_EmptyString()
    {
        var items = new List<Item>
        {
            new Item { Id = 1, Name = "Ananasas" }
        };

        var results = new ItemSimilarityService(null).FindSimilarItems("", items, 0.4);

        Assert.Empty(results);
    }
}
