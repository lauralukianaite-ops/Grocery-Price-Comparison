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

    [Fact]
    public async Task GetSimilarItemsAsync_PassesHighThresholdToIndex()
    {
        _indexMock
            .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<double>()))
            .Returns(new List<SimilarItemMatch>());
 
        var response = await CreateService().GetSimilarItemsAsync("Prancuziskas cesnakinis batonas", 0.8);
 
        Assert.Equal(0.8, response.Threshold);
        _indexMock.Verify(x => x.Find("Prancuziskas cesnakinis batonas", 0.8), Times.Once);
    }

    [Fact]
    public async Task GetSimilarItemsAsync_ReturnsLatestPricePerStore()
    {
        var item = MakeItem(1, "Apelsinų sultys 100%",
            ("Barbora", 0.99m, 10), 
            ("Barbora", 2.49m, 0),
            ("Iki", 2.99m, 5), 
            ("Iki", 2.59m, 10));

        _indexMock
            .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<double>()))
            .Returns(new List<SimilarItemMatch> { new(item, 0.9) });
 
        var response = await CreateService().GetSimilarItemsAsync("Apelsinų sultys", 0.1);
        var results = response.SimilarItems;
 
        Assert.Equal(2, results.Count);
        Assert.Equal(2.49, results.First(r => r.Store == "Barbora").Cost);
        Assert.Equal(2.99, results.First(r => r.Store == "Iki").Cost);
    }

    [Fact]
    public async Task GetSimilarItemsAsync_RoundsSimilarityScoreToTwoDecimals()
    {
        var item = MakeItem(1, "Pienas", ("Maxima", 1m, 0));
        _indexMock
            .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<double>()))
            .Returns(new List<SimilarItemMatch> { new(item, 0.8567) });
 
        var response = await CreateService().GetSimilarItemsAsync("Pienas", 0.1);
 
        Assert.Equal(0.86, response.SimilarItems.Single().SimilarityScore);
    }

    [Fact]
    public async Task GetSimilarItemsAsync_SkipsItemsWithoutPrices()
    {
        var item = MakeItem(1, "Ananasas"); // no prices
        _indexMock
            .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<double>()))
            .Returns(new List<SimilarItemMatch> { new(item, 1.0) });
 
        var response = await CreateService().GetSimilarItemsAsync("Ananasas", 0.4);
 
        Assert.Empty(response.SimilarItems);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public async Task GetSimilarItemsAsync_ThrowsOutOfRange_WhenThresholdInvalid(double threshold)
    {
        var service = CreateService();
 
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetSimilarItemsAsync("Pienas", threshold));
 
        _indexMock.Verify(x => x.Find(It.IsAny<string>(), It.IsAny<double>()), Times.Never);
    }
 
    [Fact]
    public async Task RefreshIndexAsync_LoadsItemsFromRepository_AndRebuildsIndex()
    {
        var items = new List<Item> { new Item { Id = 1, Name = "Ananasas" } };
        _repositoryMock.Setup(r => r.GetItemsWithPriceAndStoreAsync()).ReturnsAsync(items);
 
        await CreateService().RefreshIndexAsync();
 
        _indexMock.Verify(x => x.BuildOrUpdateIndex(items), Times.Once);
    }
 
    [Fact]
    public void Constructor_Throws_WhenDependenciesAreMissing()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemSimilarityService(_indexMock.Object, null!));
        Assert.Throws<ArgumentNullException>(() => new ItemSimilarityService(null!, _repositoryMock.Object));
    }
 
    [Fact]
    public void CalculateSimilarity_ReturnsOne_ForIdenticalOrDiacriticEquivalentStrings()
    {
        var service = CreateService();
 
        Assert.Equal(1.0, service.CalculateSimilarity("Dvaro pienas", "dvaro pienas"), precision: 2);
        Assert.Equal(1.0, service.CalculateSimilarity("Česnakinis batonas", "cesnakinis batonas"), precision: 2);
        Assert.Equal(1.0, service.CalculateSimilarity("Šviežia varškė 9%", "Sviezia varske 9%"), precision: 2);
    }
 
    [Fact]
    public void CalculateSimilarity_ReturnsZero_WhenInputsAreNullOrEmpty()
    {
        var service = CreateService();
 
        Assert.Equal(0.0, service.CalculateSimilarity(null!, "Pienas"));
        Assert.Equal(0.0, service.CalculateSimilarity("Pienas", null!));
        Assert.Equal(0.0, service.CalculateSimilarity("", "Pienas"));
        Assert.Equal(0.0, service.CalculateSimilarity("   ", "Pienas"));
    }
}