using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Entities;
using backend.Repositories;
using backend.Services;
using Moq;
using Xunit;

namespace backend.Tests;

public class ItemSimilarityServiceIndexIntegrationTests
{
    private readonly Mock<IItemsRepository> _repositoryMock = new();

    private static Item MakeItem(int id, string name, params (string store, decimal cost, int daysAgo)[] prices) =>
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

    private async Task<ItemSimilarityService> CreateLoadedServiceAsync(params Item[] items)
    {
        _repositoryMock
            .Setup(r => r.GetItemsWithPriceAndStoreAsync())
            .ReturnsAsync(items.ToList());

        var service = new ItemSimilarityService(new ItemSimilarityIndex(), _repositoryMock.Object);
        await service.RefreshIndexAsync();
        return service;
    }

    [Fact]
    public async Task Search_ReturnsMilkVariants_AndExcludesBread()
    {
        var service = await CreateLoadedServiceAsync(
            MakeItem(2, "Pienas 2.5% (500ml)", ("Maxima", 1.29m, 0)),
            MakeItem(3, "Skystas Pienas 2.5% 1L", ("Iki", 1.49m, 0)),
            MakeItem(4, "Duona šviesi", ("Rimi", 1.00m, 0)),
            MakeItem(5, "Pienas 3.5% 1l", ("Barbora", 1.59m, 0)));

        var response = await service.GetSimilarItemsAsync("Pienas", 0.1);

        Assert.Contains(response.SimilarItems, r => r.Id == 2);
        Assert.Contains(response.SimilarItems, r => r.Id == 3);
        Assert.Contains(response.SimilarItems, r => r.Id == 5);
        Assert.DoesNotContain(response.SimilarItems, r => r.Id == 4);
    }

    [Fact]
    public async Task Search_WithHighThreshold_ReturnsNearDuplicates_AndExcludesUnrelated()
    {
        var service = await CreateLoadedServiceAsync(
            MakeItem(1, "Prancūziškas česnakinis batonas", ("Maxima", 2.00m, 0)),
            MakeItem(2, "Batonas prancūziškas česnakinis", ("Iki", 2.10m, 0)),
            MakeItem(3, "Samsung ultra hd max pro phone", ("Lidl", 999.00m, 0)));

        var response = await service.GetSimilarItemsAsync("Prancuziskas cesnakinis batonas", 0.8);

        Assert.Contains(response.SimilarItems, r => r.Id == 1);
        Assert.Contains(response.SimilarItems, r => r.Id == 2);
        Assert.DoesNotContain(response.SimilarItems, r => r.Id == 3);
    }

    [Fact]
    public async Task Search_ReturnsLatestPricePerStore_ForMatchedItem()
    {
        var service = await CreateLoadedServiceAsync(
            MakeItem(1, "Apelsinų sultys 100%",
                ("Barbora", 0.99m, 10), ("Barbora", 2.49m, 0),
                ("Iki", 2.99m, 5), ("Iki", 2.59m, 10)));

        var response = await service.GetSimilarItemsAsync("Apelsinų sultys", 0.1);
        var results = response.SimilarItems;

        Assert.Equal(2, results.Count);
        Assert.Equal(2.49, results.First(r => r.Store == "Barbora").Cost);
        Assert.Equal(2.99, results.First(r => r.Store == "Iki").Cost);
    }

    [Fact]
    public async Task Search_ReturnsBestMatchFirst_WithScoreOfOneForExactName()
    {
        var service = await CreateLoadedServiceAsync(
            MakeItem(1, "Pienas 2.5% 1l dvaro", ("Maxima", 1.0m, 0)),
            MakeItem(2, "Pienas", ("Iki", 1.0m, 0)));

        var response = await service.GetSimilarItemsAsync("Pienas", 0.1);

        Assert.Equal(2, response.SimilarItems.First().Id);
        Assert.Equal(1.0, response.SimilarItems.First().SimilarityScore);
    }

    [Fact]
    public async Task Search_IsDiacriticInsensitive()
    {
        var service = await CreateLoadedServiceAsync(
            MakeItem(1, "Šviežia varškė 9%", ("Maxima", 1.5m, 0)));

        var response = await service.GetSimilarItemsAsync("sviezia varske", 0.3);

        Assert.Single(response.SimilarItems);
    }

    [Fact]
    public async Task Search_ReturnsEmpty_WhenIndexWasNeverRefreshed()
    {
        var service = new ItemSimilarityService(new ItemSimilarityIndex(), _repositoryMock.Object);

        var response = await service.GetSimilarItemsAsync("Pienas", 0.1);

        Assert.Empty(response.SimilarItems);
    }

    [Fact]
    public async Task RefreshIndexAsync_ReplacesOldData_WithNewRepositoryData()
    {
        var service = await CreateLoadedServiceAsync(
            MakeItem(1, "Pienas", ("Maxima", 1.0m, 0)));

        _repositoryMock
            .Setup(r => r.GetItemsWithPriceAndStoreAsync())
            .ReturnsAsync(new List<Item> { MakeItem(2, "Duona", ("Rimi", 1.0m, 0)) });
        await service.RefreshIndexAsync();

        var milk = await service.GetSimilarItemsAsync("Pienas", 0.1);
        var bread = await service.GetSimilarItemsAsync("Duona", 0.1);

        Assert.Empty(milk.SimilarItems);
        Assert.Single(bread.SimilarItems);
    }

    [Fact]
    public async Task Search_ThrowsArgumentException_ForEmptyName()
    {
        var service = await CreateLoadedServiceAsync(
            MakeItem(1, "Ananasas", ("Maxima", 1.0m, 0)));

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetSimilarItemsAsync("", 0.4));
    }
}