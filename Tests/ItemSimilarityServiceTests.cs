using System;
using System.Collections.Generic;
using System.Linq;
using backend.Entities;
using backend.Services;
using Xunit;
using System.Threading.Tasks;
using Moq;
using backend.Repositories;

using backend.DTOs;
using backend.Services.Interfaces;

namespace backend.Tests;

//TESTS: validation, Dto mapping

public class ItemSimilarityServiceTests
{
    private readonly Mock<IItemSimilarityIndex> _indexMock = new();
    private readonly Mock<IItemsRepository> _repositoryMock = new();

    private ItemSimilarityService CreateService()
    {
        return new ItemSimilarityService(
            _indexMock.Object,
            _repositoryMock.Object
        );
    }

    private static Item MakeItem (int id, string name, params (string store, decimal cost, int daysAgo)[] prices) => 
        new Item
        {
            Id = id,
            Name = name,
            Prices = prices.Select(p => new Price
            {
                Cost = p.cost,
                RecordedAt = DateTime.Now.AddDays(-p.daysAgo),
                Store = new Store { Name = p.store }
            }).ToList()
        };

    [Fact]
    public async Task GetSimilarItemsAsync_MapsToDtos_AndWrapsInResponse()
    {
        var fromIndex = new List<SimilarItemMatch>
        {
            new(MakeItem(2, "Pienas 2.5% (500ml)", ("Maxima", 1.29m, 0)), 0.8),
            new(MakeItem(3, "Skystas Pienas 2.5% 1L", ("Iki", 1.49m, 0)), 0.6),
            new(MakeItem(5, "Pienas 3.5% 1l", ("Barbora", 1.59m, 0)), 0.7)
        };

         _indexMock.Setup(x => x.Find("Pienas", 0.1)).Returns(fromIndex);

        var response = await CreateService().GetSimilarItemsAsync("Pienas", 0.1);
 
        Assert.Equal("Pienas", response.BaseItemName);
        Assert.Equal(0.1, response.Threshold);
        Assert.Equal(3, response.SimilarItems.Count);
        Assert.Contains(response.SimilarItems, r => r.Id == 2 && r.Store == "Maxima");
        Assert.Contains(response.SimilarItems, r => r.Id == 3);
        Assert.Contains(response.SimilarItems, r => r.Id == 5);
        _indexMock.Verify(x => x.Find("Pienas", 0.1), Times.Once);
    }

    
}