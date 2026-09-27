using System;
using System.Collections.Generic;
using System.Linq;
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
        var searchQuery = "Pieno";
        var items = new List<Item>
        {
            new Item { Id = 2, Name = "Pienas 2.5% (500ml)", Prices = new List<Price> { new Price { Cost = 1.29m, RecordedAt = DateTime.Now, Store = new Store { Name = "Maxima" } } } },
            new Item { Id = 3, Name = "Skystas Pienas 2.5% 1L", Prices = new List<Price> { new Price { Cost = 1.49m, RecordedAt = DateTime.Now, Store = new Store { Name = "Iki" } } } },
            new Item { Id = 4, Name = "Duona šviesi", Prices = new List<Price> { new Price { Cost = 1.00m, RecordedAt = DateTime.Now, Store = new Store { Name = "Rimi" } } } },
            new Item { Id = 5, Name = "Pienas 3.5% 1l", Prices = new List<Price> { new Price { Cost = 1.59m, RecordedAt = DateTime.Now, Store = new Store { Name = "Barbora" } } } }
        };

        var results = new ItemSimilarityService(null).FindSimilarItems(searchQuery, items, 0.1);

        foreach (var r in results)
        {
            Console.WriteLine($"  [Similarity index: {r.SimilarityScore:F2}] Id: {r.Id} - {r.Name}");
        }

        Assert.Contains(results, r => r.Id == 2);
        Assert.Contains(results, r => r.Id == 3);
        Assert.DoesNotContain(results, r => r.Id == 4);
        Assert.Contains(results, r => r.Id == 5);
        Assert.True(results.Count >= 2);
    }

    [Fact]
    public void FindSimilarItems_WithHighTreshold()
    {
        var searchQuery = "Prancūziškas česnakinis batonas";
        var items = new List<Item>
        {
            new Item { Id = 1, Name = "Prancūziškas česnakinis batonas", Prices = new List<Price> { new Price { Cost = 2.00m, RecordedAt = DateTime.Now, Store = new Store { Name = "Maxima" } } }},
            new Item { Id = 2, Name = "Batonas prancūziškas česnakinis", Prices = new List<Price> { new Price { Cost = 2.10m, RecordedAt = DateTime.Now, Store = new Store { Name = "Iki" } } }},
            new Item { Id = 3, Name = "Samsung ultra hd max pro phone", Prices = new List<Price> { new Price { Cost = 999.00m, RecordedAt = DateTime.Now, Store = new Store { Name = "Lidl" } } }}
        };

        var results = new ItemSimilarityService(null).FindSimilarItems(searchQuery, items, 0.8);

        foreach (var r in results)
        {
            Console.WriteLine($"  [Similarity index: {r.SimilarityScore:F2}] Id: {r.Id} - {r.Name}");
        }


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
        Console.WriteLine(string.Join(", ", results.Select(r => $"{r.Id}:{r.Name}:{r.SimilarityScore}")));
    }

    [Fact]
    public void FindSimilarItems_ReturnStoreAndPrice()
    {
        var searchQuery = "Apelsinų sultys";
        var items = new List<Item>
        {
            new Item
            {
                Id = 1,
                Name = "Apelsinų sultys 100%",
                Prices = new List<Price>
                {
                    new Price { Cost = 0.99m, RecordedAt = DateTime.Now.AddDays(-10), Store = new Store { Name = "Barbora" } },
                    new Price { Cost = 2.49m, RecordedAt = DateTime.Now, Store = new Store { Name = "Barbora" } },

                    new Price { Cost = 2.99m, RecordedAt = DateTime.Now.AddDays(-5), Store = new Store { Name = "Iki" } },
                    new Price { Cost = 2.59m, RecordedAt = DateTime.Now.AddDays(-10), Store = new Store { Name = "Iki" } }
                }
            },
        };
        var results = new ItemSimilarityService(null).FindSimilarItems(searchQuery, items, 0.1);

        Assert.Equal(2, results.Count);
        
        var barboraResult = results.First(r => r.Store == "Barbora");
        Assert.Equal(2.49, barboraResult.Cost);

        var ikiResult = results.First(r => r.Store == "Iki");
        Assert.Equal(2.99, ikiResult.Cost);

        foreach (var r in results)
        {
            Console.WriteLine($"  [Similarity index: {r.SimilarityScore:F2}] Id: {r.Id} - {r.Name}; Store: {r.Store}; Price: {r.Cost}");
        }
    }
}