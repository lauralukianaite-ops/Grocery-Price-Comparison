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

    [Fact]
    public async Task Pagination_ReturnsRequestedSliceAndFullTotalCount()
    {
        var (context, repo) = await SeedAsync();
        await using var _ = context;

        var result = await repo.GetCatalogPageAsync(Query(CatalogSort.Name, page: 2, pageSize: 2));

        Assert.Equal(3, result.TotalCount);
        Assert.Equal("Date", Assert.Single(result.Items).Name);
    }
}