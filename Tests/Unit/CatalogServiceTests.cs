using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Entities;
using backend.Enums;
using backend.Repositories;
using backend.Services;
using backend.ValueObjects;
using Moq;
using Xunit;

namespace backend.Tests;

public class CatalogServiceTests
{
    private readonly Mock<IItemsRepository> _items = new();
    private readonly Mock<IStoresRepository> _stores = new();
    private readonly CatalogService _service;

    public CatalogServiceTests()
    {
        _service = new CatalogService(_items.Object, _stores.Object);
    }

    [Fact]
    public async Task GetCatalogAsync_ClampsPageAndPageSizeBeforeCallingRepository()
    {
          _items.Setup(r => r.GetLatestPricesWithStoreAsync(It.IsAny<string>(), It.IsAny<List<int>>()))
              .ReturnsAsync(new List<Price>());
          _items.Setup(r => r.GetItemsAsync())
              .ReturnsAsync(new List<Item>());

        var response = await _service.GetCatalogAsync(
            new CatalogQuery(0, 1000, null, null, CatalogSort.Name, null));

        Assert.Equal(1, response.Page);
        Assert.Equal(100, response.PageSize);
          _items.Verify(r => r.GetLatestPricesWithStoreAsync(null, null), Times.Once);
          _items.Verify(r => r.GetItemsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCatalogAsync_MapsOffersOrderedByStoreName()
    {
        var iki = new Store { Id = 1, Name = "Iki" };
        var rimi = new Store { Id = 2, Name = "Rimi" };
        var item = new Item
        {
            Id = 1, Name = "Milk",
            Prices =
            [
                new Price { ItemId = 1, StoreId = 2, Store = rimi, Cost = 2m, RecordedAt = DateTime.UtcNow },
                new Price { ItemId = 1, StoreId = 1, Store = iki, Cost = 1m, RecordedAt = DateTime.UtcNow }
            ]
        };
          _items.Setup(r => r.GetLatestPricesWithStoreAsync(It.IsAny<string>(), It.IsAny<List<int>>()))
              .ReturnsAsync(item.Prices);
          _items.Setup(r => r.GetItemsAsync())
              .ReturnsAsync([new Item { Id = item.Id, Name = item.Name }]);

        var response = await _service.GetCatalogAsync(
            new CatalogQuery(1, 12, null, null, CatalogSort.Name, null));

        var dto = Assert.Single(response.Items);
        Assert.Equal(["Iki", "Rimi"], dto.Offers.Select(o => o.StoreName));
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task GetStoresAsync_MapsEntitiesToDtos()
    {
        _stores.Setup(r => r.GetAllAsync())
               .ReturnsAsync([new Store { Id = 1, Name = "Iki" }]);

        var stores = await _service.GetStoresAsync();

        var dto = Assert.Single(stores);
        Assert.Equal(1, dto.Id);
        Assert.Equal("Iki", dto.Name);
    }
}
