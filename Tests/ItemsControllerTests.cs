using System;
using System.Threading.Tasks;
using backend.Controllers;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests;

public class ItemsControllerTests
{
    [Fact]
    public async Task GetItems_ReturnsRequestedPageWithLatestStoreOffer()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var store = new Store { Id = 1, Name = "Test store" };
        context.Items.AddRange(
            new Item { Id = 1, Name = "Apple" },
            new Item
            {
                Id = 2,
                Name = "Banana",
                Prices =
                [
                    new Price
                    {
                        Id = 1,
                        StoreId = store.Id,
                        Store = store,
                        Cost = 10.00m,
                        RecordedAt = new DateTime(2026, 1, 1)
                    },
                    new Price
                    {
                        Id = 2,
                        StoreId = store.Id,
                        Store = store,
                        Cost = 1.50m,
                        RecordedAt = new DateTime(2026, 1, 2)
                    }
                ]
            },
            new Item
            {
                Id = 3,
                Name = "Cherry",
                Prices =
                [
                    new Price
                    {
                        Id = 3,
                        StoreId = store.Id,
                        Store = store,
                        Cost = 2.00m,
                        RecordedAt = new DateTime(2026, 1, 2)
                    }
                ]
            });
        await context.SaveChangesAsync();

        var repository = new ItemsRepository(context);
        var controller = new ItemsController(null!, null!, null!, new CatalogService(repository));

        var actionResult = await controller.GetItems(page: 2, pageSize: 1, sort: "price-high");

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<CatalogResponseDto>(okResult.Value);
        var item = Assert.Single(response.Items);
        var offer = Assert.Single(item.Offers);

        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal("Banana", item.Name);
        Assert.Equal(1.50m, offer.Cost);
    }
}

