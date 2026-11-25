#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Entities;
using backend.Enums;
using backend.Repositories;
using backend.Services;
using backend.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests;

public class CatalogServiceIntegrationTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CatalogQuery Query(
        CatalogSort sort = CatalogSort.DiscountHigh,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? store = null,
        List<int>? ids = null) =>
        new(page, pageSize, search, store, sort, ids);

    private static async Task<(AppDbContext Context, CatalogService Service)> SeedAsync()
    {
        var context = CreateContext();
        var iki = new Store { Id = 1, Name = "Iki" };
        var rimi = new Store { Id = 2, Name = "Rimi" };

        context.Items.AddRange(
            new Item { Id = 1, Name = "Ananasas" },
            new Item
            {
                Id = 2, Name = "Bananas",
                Prices =
                [
                    new Price { Id = 1, StoreId = 1, Store = iki, Cost = 5m, RetailCost = 5m, RecordedAt = new DateTime(2026, 1, 1) },
                    new Price { Id = 2, StoreId = 1, Store = iki, Cost = 1m, RetailCost = 2m, RecordedAt = new DateTime(2026, 1, 2) },
                    new Price { Id = 3, StoreId = 2, Store = rimi, Cost = 1.5m, RetailCost = null, RecordedAt = new DateTime(2026, 1, 2) }
                ]
            },
            new Item
            {
                Id = 3, Name = "Citrina",
                Prices =
                [
                    new Price { Id = 4, StoreId = 2, Store = rimi, Cost = 9m, RetailCost = 10m, RecordedAt = new DateTime(2026, 1, 2) }
                ]
            },
            new Item
            {
                Id = 4, Name = "Datule",
                Prices =
                [
                    new Price { Id = 5, StoreId = 1, Store = iki, Cost = 3m, RetailCost = null, RecordedAt = new DateTime(2026, 1, 2) }
                ]
            });
        await context.SaveChangesAsync();
        return (context, new CatalogService(new ItemsRepository(context), new StoresRepository(context)));
    }

    [Fact]
    public async Task GetCatalogAsync_WhenItemsHaveNoPrices_ExcludeThem()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query());

        Assert.Equal(3, result.TotalCount);
        Assert.DoesNotContain(result.Items, i => i.Name == "Ananasas");
    }

    [Fact]
    public async Task GetCatalogAsync_WhenMultiplePricesExistForStore_ReturnsOnlyLatestPricePerStore()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(ids: [2]));

        var banana = Assert.Single(result.Items);
        Assert.Equal(2, banana.Offers.Count);
        Assert.DoesNotContain(banana.Offers, p => p.Cost == 5m);
    }

    [Fact]
    public async Task GetCatalogAsync_WhenSortIsDiscountHigh_OrdersByBiggestDiscountFirst()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(CatalogSort.DiscountHigh));

        Assert.Equal(["Bananas", "Citrina", "Datule"], result.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task GetCatalogAsync_WhenSortIsPriceLow_OrdersByBestPriceAscending()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(CatalogSort.PriceLow));

        Assert.Equal(["Bananas", "Datule", "Citrina"], result.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task GetCatalogAsync_WhenSortIsPriceHigh_OrdersByBestPriceDescending()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(CatalogSort.PriceHigh));

        Assert.Equal(["Citrina", "Datule", "Bananas"], result.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task GetCatalogAsync_WhenSortIsName_OrdersAlphabetically()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(CatalogSort.Name));

        Assert.Equal(["Bananas", "Citrina", "Datule"], result.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task GetCatalogAsync_WhenStoreFilterIsProvided_ReturnsOnlyThatStoresOffersCaseInsensitively()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(store: "rimi"));

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, i => Assert.All(i.Offers, p => Assert.Equal("Rimi", p.StoreName)));
    }

    [Fact]
    public async Task GetCatalogAsync_WhenSearchTermIsProvided_FiltersByNameCaseInsensitively()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(search: "  CITR "));

        var item = Assert.Single(result.Items);
        Assert.Equal("Citrina", item.Name);
    }

    [Theory]
    [InlineData("Bulvės", "Bulvės", "Bulves")]
    [InlineData("Bulves", "Bulvės", "Bulves")]
    [InlineData("  BULVĖS  ", "BULVĖS", "BULVES")]
    [InlineData("BULVES", "BULVĖS", "BULVES")]
    [InlineData("vės", "Bulvės", "Bulves")]
    [InlineData("ves", "Bulvės", "Bulves")]
    [InlineData("Bulve\u0307s", "Bulvės", "Bulves")]
    [InlineData("ąčęėįšųūž", "ĄČĘĖĮŠŲŪŽ", "ACEEISUUZ")]
    [InlineData("aceeisuuz", "ĄČĘĖĮŠŲŪŽ", "ACEEISUUZ")]
    public async Task GetCatalogAsync_WhenSearchContainsLithuanianCharacters_MatchesAccentedAndPlainNamesBeforePagination(
        string search, string accentedName, string plainName)
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;
        (await context.Items.FindAsync(2))!.Name = accentedName;
        (await context.Items.FindAsync(3))!.Name = plainName;
        await context.SaveChangesAsync();

        var firstPage = await service.GetCatalogAsync(Query(pageSize: 1, search: search));
        var secondPage = await service.GetCatalogAsync(Query(page: 2, pageSize: 1, search: search));

        Assert.Equal(2, firstPage.TotalCount);
        Assert.Equal(2, secondPage.TotalCount);
        var firstItem = Assert.Single(firstPage.Items);
        var secondItem = Assert.Single(secondPage.Items);
        Assert.Equal(2, firstItem.Id);
        Assert.Equal(accentedName, firstItem.Name);
        Assert.Equal(3, secondItem.Id);
        Assert.Equal(plainName, secondItem.Name);
    }

    [Fact]
    public async Task GetCatalogAsync_WhenPaginationIsApplied_ReturnsRequestedSliceAndFullTotalCount()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(CatalogSort.Name, page: 2, pageSize: 2));

        Assert.Equal(3, result.TotalCount);
        Assert.Equal("Datule", Assert.Single(result.Items).Name);
    }

    [Theory]
    [InlineData(CatalogSort.NameDescending)]
    [InlineData(CatalogSort.DiscountLow)]
    public async Task GetCatalogAsync_WhenSecondarySortDirectionsAreUsed_SortsBeforePagination(CatalogSort sort)
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(Query(sort));
        var secondPage = await service.GetCatalogAsync(Query(sort, page: 2, pageSize: 1));

        Assert.Equal(new[] { "Datule", "Citrina", "Bananas" }, result.Items.Select(item => item.Name));
        Assert.Equal("Citrina", Assert.Single(secondPage.Items).Name);
        Assert.Equal(3, secondPage.TotalCount);
    }

    [Fact]
    public async Task GetCatalogAsync_WhenStoreIdsAreProvided_FiltersCurrentOffersBeforePagination()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var result = await service.GetCatalogAsync(
            Query(CatalogSort.Name, page: 2, pageSize: 1) with { StoreIds = [2] });

        Assert.Equal(2, result.TotalCount);
        var cherry = Assert.Single(result.Items);
        Assert.Equal("Citrina", cherry.Name);
        Assert.All(cherry.Offers, price => Assert.Equal(2, price.StoreId));

        var bothStores = await service.GetCatalogAsync(Query() with { StoreIds = [1, 2] });
        Assert.Equal(3, bothStores.TotalCount);
    }

    [Fact]
    public async Task GetCatalogAsync_WhenPriceRangeIsProvided_FiltersByInclusiveBestPriceBoundsBeforePagination()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;

        var query = Query(CatalogSort.PriceLow, page: 2, pageSize: 1) with
        {
            MinPrice = 1m,
            MaxPrice = 3m
        };
        var result = await service.GetCatalogAsync(query);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Datule", Assert.Single(result.Items).Name);

        var empty = await service.GetCatalogAsync(Query() with { MaxPrice = 0.5m });
        Assert.Equal(0, empty.TotalCount);
        Assert.Empty(empty.Items);
    }

    [Fact]
    public async Task GetCatalogAsync_WhenDiscountedOnlyIsTrue_MatchesDiscountsAtAnySelectedStore()
    {
        var (context, service) = await SeedAsync();
        await using var _ = context;
        context.Prices.Add(new Price
        {
            ItemId = 3,
            StoreId = 1,
            Cost = 1m,
            RetailCost = 1m,
            RecordedAt = new DateTime(2026, 1, 3)
        });
        await context.SaveChangesAsync();

        var result = await service.GetCatalogAsync(
            Query(page: 2, pageSize: 1) with { DiscountedOnly = true });
        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Citrina", Assert.Single(result.Items).Name);

        var selectedStore = await service.GetCatalogAsync(Query() with
        {
            DiscountedOnly = true,
            StoreIds = [1]
        });
        Assert.Equal("Bananas", Assert.Single(selectedStore.Items).Name);
    }
}
