using System.Collections.Generic;
using backend.Entities;
using backend.Services;
using Xunit;

namespace backend.Tests;

public class ItemSimilarityServiceTests
{
    [Fact]
    public void FindSimilarItems_ReturnsExpectedMilkVariants()
    {
        var service = new ItemSimilarityService();

        var baseItem = new Item { Id = 1, Name = "Pienas 2.5% (1L)" };

        var items = new List<Item>
        {
            baseItem,
            new Item { Id = 2, Name = "Pienas 2.5% (500ml)" },
            new Item { Id = 3, Name = "Skystas Pienas 2.5% 1L" },
            new Item { Id = 4, Name = "Duona šviesi" },
            new Item { Id = 5, Name = "Pienas 3.5% 1l" }
        };

        var results = service.FindSimilarItems(baseItem, items, 0.3);

        Assert.Contains(results, r => r.Id == 2);
        Assert.Contains(results, r => r.Id == 3);
        Assert.DoesNotContain(results, r => r.Id == 4);
        Assert.Contains(results, r => r.Id == 5);
        Assert.True(results.Count >= 2);
    }
}
