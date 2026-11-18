using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Entities;
using backend.Enums;
using backend.Repositories;
using backend.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests;

public class ItemsRepositoryCatalogTests
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

    private static async Task<(AppDbContext Context, ItemsRepository Repo)> SeedAsync()
    {
        var context = CreateContext();
        var iki = new Store { Id = 1, Name = "Iki" };
        var rimi = new Store { Id = 2, Name = "Rimi" };

        context.Items.AddRange(
            new Item { Id = 1, Name = "Apple" },
            new Item
            {
                Id = 2, Name = "Banana",
                Prices =
                [
                    new Price { Id = 1, StoreId = 1, Store = iki, Cost = 5m, RetailCost = 5m, RecordedAt = new DateTime(2026, 1, 1) },
                    new Price { Id = 2, StoreId = 1, Store = iki, Cost = 1m, RetailCost = 2m, RecordedAt = new DateTime(2026, 1, 2) }, // 50 %
                    new Price { Id = 3, StoreId = 2, Store = rimi, Cost = 1.5m, RetailCost = null, RecordedAt = new DateTime(2026, 1, 2) }
                ]
            },
            new Item
            {
                Id = 3, Name = "Cherry",
                Prices =
                [
                    new Price { Id = 4, StoreId = 2, Store = rimi, Cost = 9m, RetailCost = 10m, RecordedAt = new DateTime(2026, 1, 2) } // 10 %
                ]
            },
            new Item
            {
                Id = 4, Name = "Date",
                Prices =
                [
                    new Price { Id = 5, StoreId = 1, Store = iki, Cost = 3m, RetailCost = null, RecordedAt = new DateTime(2026, 1, 2) } // 0 %
                ]
            });
        await context.SaveChangesAsync();
        return (context, new ItemsRepository(context));
    }

    [Fact]
    public async Task ExcludesItemsWithoutPrices()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query());

        Assert.Equal(3, result.TotalCount);
        Assert.DoesNotContain(result.Items, i => i.Name == "Apple");
    }

    [Fact]
    public async Task ReturnsOnlyLatestPricePerStore()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(ids: [2]));

        var banana = Assert.Single(result.Items);
        Assert.Equal(2, banana.Prices.Count);
        Assert.DoesNotContain(banana.Prices, p => p.Cost == 5m);
    }

    [Fact]
    public async Task DiscountHigh_OrdersByBiggestDiscountFirst()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(CatalogSort.DiscountHigh));

        Assert.Equal(["Banana", "Cherry", "Date"], result.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task PriceLow_OrdersByBestPriceAscending()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(CatalogSort.PriceLow));

        Assert.Equal(["Banana", "Date", "Cherry"], result.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task PriceHigh_OrdersByBestPriceDescending()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(CatalogSort.PriceHigh));

        Assert.Equal(["Cherry", "Date", "Banana"], result.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task Name_OrdersAlphabetically()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(CatalogSort.Name));

        Assert.Equal(["Banana", "Cherry", "Date"], result.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task StoreFilter_IsCaseInsensitiveAndReturnsOnlyThatStoresOffers()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(store: "rimi"));

        Assert.Equal(2, result.TotalCount); // Banana ir Cherry
        Assert.All(result.Items, i => Assert.All(i.Prices, p => Assert.Equal("Rimi", p.Store!.Name)));
    }

    [Fact]
    public async Task Search_FiltersByNameCaseInsensitively()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(search: "  CHER "));

        var item = Assert.Single(result.Items);
        Assert.Equal("Cherry", item.Name);
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
    public async Task Search_MatchesLithuanianAndPlainNamesBeforePagination(
        string search, string accentedName, string plainName)
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;
        (await context.Items.FindAsync(2))!.Name = accentedName;
        (await context.Items.FindAsync(3))!.Name = plainName;
        await context.SaveChangesAsync();

        var firstPage = await repo.GetCatalogPageAsync(Query(pageSize: 1, search: search));
        var secondPage = await repo.GetCatalogPageAsync(Query(page: 2, pageSize: 1, search: search));

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
    public async Task Pagination_ReturnsRequestedSliceAndFullTotalCount()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(CatalogSort.Name, page: 2, pageSize: 2));

        Assert.Equal(3, result.TotalCount);
        Assert.Equal("Date", Assert.Single(result.Items).Name);
    }

    [Theory]
    [InlineData(CatalogSort.NameDescending)]
    [InlineData(CatalogSort.DiscountLow)]
    public async Task AdditionalSortDirections_AreAppliedBeforePagination(CatalogSort sort)
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(sort));
        var secondPage = await repo.GetCatalogPageAsync(Query(sort, page: 2, pageSize: 1));

        Assert.Equal(new[] { "Date", "Cherry", "Banana" }, result.Items.Select(item => item.Name));
        Assert.Equal("Cherry", Assert.Single(secondPage.Items).Name);
        Assert.Equal(3, secondPage.TotalCount);
    }

    [Fact]
    public async Task StoreIds_FilterCurrentOffersBeforePagination()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(
            Query(CatalogSort.Name, page: 2, pageSize: 1) with { StoreIds = [2] });

        Assert.Equal(2, result.TotalCount);
        var cherry = Assert.Single(result.Items);
        Assert.Equal("Cherry", cherry.Name);
        Assert.All(cherry.Prices, price => Assert.Equal(2, price.StoreId));

        var bothStores = await repo.GetCatalogPageAsync(Query() with { StoreIds = [1, 2] });
        Assert.Equal(3, bothStores.TotalCount);
    }

    [Fact]
    public async Task PriceRange_UsesInclusiveBestPriceBoundsBeforePagination()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var query = Query(CatalogSort.PriceLow, page: 2, pageSize: 1) with
        {
            MinPrice = 1m,
            MaxPrice = 3m
        };
        var result = await repo.GetCatalogPageAsync(query);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Date", Assert.Single(result.Items).Name);

        var empty = await repo.GetCatalogPageAsync(Query() with { MaxPrice = 0.5m });
        Assert.Equal(0, empty.TotalCount);
        Assert.Empty(empty.Items);
    }

    [Fact]
    public async Task DiscountedOnly_MatchesDiscountsAtAnySelectedStore()
    {
        var (context, repo) = await SeedAsync();
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

        // Cherry's cheapest offer has no discount, but its other current offer does.
        var result = await repo.GetCatalogPageAsync(
            Query(page: 2, pageSize: 1) with { DiscountedOnly = true });
        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Cherry", Assert.Single(result.Items).Name);

        var selectedStore = await repo.GetCatalogPageAsync(Query() with
        {
            DiscountedOnly = true,
            StoreIds = [1]
        });
        Assert.Equal("Banana", Assert.Single(selectedStore.Items).Name);
    }
}
