using System;
using System.Collections.Generic;
using System.Linq;
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

namespace backend.Tests.Integration;

// Real controller, service and repositories on an in-memory database; only Google is replaced.
public class BasketIntegrationTests
{
    private const double UserLatitude = 54.6872;
    private const double UserLongitude = 25.2797;
    private const int Milk = 1;
    private const int Bread = 2;
    private const int Kefir = 3;

    private readonly AppDbContext _context;
    private readonly FixedDistanceProvider _distances = new();
    private readonly BasketsController _controller;

    // xUnit builds a new instance for every test, so every test gets its own empty database.
    public BasketIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        Seed();

        var itemsRepository = new ItemsRepository(_context);
        var basketsRepository = new BasketsRepository(_context);
        var service = new BasketService(
            itemsRepository,
            basketsRepository,
            new FakeDiscountService(itemsRepository),
            new ItemSimilarityService(new ItemSimilarityIndex()),
            _distances);
        _controller = new BasketsController(service);
    }

    // Milk + bread: Rimi 19.00 (shop 111 km away), Maxima 20.00 (road 34 km), Iki 22.00 (road 3 km); Assorti has no bread.
    private void Seed()
    {
        _context.Items.AddRange(
            new Item { Id = Milk, Name = "pienas" },
            new Item { Id = Bread, Name = "duona" },
            new Item { Id = Kefir, Name = "kefyras" });

        _context.Stores.AddRange(
            new Store { Id = 1, Name = "Rimi" },
            new Store { Id = 2, Name = "Maxima" },
            new Store { Id = 3, Name = "Iki" },
            new Store { Id = 4, Name = "Assorti" });

        _context.Prices.AddRange(
            new Price { StoreId = 1, ItemId = Milk, Cost = 9.00m },
            new Price { StoreId = 1, ItemId = Bread, Cost = 10.00m },
            new Price { StoreId = 2, ItemId = Milk, Cost = 10.00m },
            new Price { StoreId = 2, ItemId = Bread, Cost = 10.00m },
            new Price { StoreId = 3, ItemId = Milk, Cost = 11.00m },
            new Price { StoreId = 3, ItemId = Bread, Cost = 11.00m },
            new Price { StoreId = 4, ItemId = Milk, Cost = 1.00m });

        // 1 degree of latitude is about 111 km, so these shops are 111 km, 11 km and 1.1 km north of the user.
        _context.StoreLocations.AddRange(
            new StoreLocation { StoreId = 1, Address = "Rimi street 1", Latitude = 55.6872, Longitude = UserLongitude },
            new StoreLocation { StoreId = 2, Address = "Maxima street 2", Latitude = 54.7872, Longitude = UserLongitude },
            new StoreLocation { StoreId = 3, Address = "Iki street 3", Latitude = 54.6972, Longitude = UserLongitude });

        _context.SaveChanges();

        _distances.RoadKm.Add(("Rimi street 1", 150));
        _distances.RoadKm.Add(("Maxima street 2", 34));
        _distances.RoadKm.Add(("Iki street 3", 3));
    }

    private static BasketRequestDto MilkAndBread(double? latitude = null, double? longitude = null, double? maxKm = null)
    {
        var items = new List<BasketRequestItemDto> { new(Milk, 1), new(Bread, 1) };
        return new BasketRequestDto(items, "normal", null, latitude, longitude, null, maxKm);
    }

    private async Task<BasketResponseDto> CreateOk(BasketRequestDto request)
    {
        var result = await _controller.Create(request);
        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsType<BasketResponseDto>(ok.Value);
    }

    private async Task<string> Create422(BasketRequestDto request)
    {
        var result = await _controller.Create(request);
        var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result);
        return Assert.IsType<string>(unprocessable.Value);
    }

    // No location: Rimi wins on groceries alone (19.00), nothing about the trip is filled in and Google is never asked.
    [Fact]
    public async Task WithoutLocation_CheapestGroceriesWin_AndNoDistancesAreAsked()
    {
        var basket = await CreateOk(MilkAndBread());

        Assert.Equal("Rimi", basket.StoreName);
        Assert.Equal(19.00m, basket.GroceriesTotal);
        Assert.Equal(0m, basket.TravelCost);
        Assert.Equal(19.00m, basket.GrandTotal);
        Assert.Null(basket.DistanceKm);
        Assert.Null(basket.LocationAddress);
        Assert.Equal(0, _distances.Calls);
    }

    // With a location: Rimi is 111 km away and out. Maxima 20.00 + 13.60 = 33.60, Iki 22.00 + 1.20 = 23.20, so Iki wins.
    [Fact]
    public async Task WithLocation_LowestGroceriesPlusTravelWins()
    {
        var basket = await CreateOk(MilkAndBread(UserLatitude, UserLongitude));

        Assert.Equal("Iki", basket.StoreName);
        Assert.Equal(22.00m, basket.GroceriesTotal);
        Assert.Equal(1.20m, basket.TravelCost);
        Assert.Equal(23.20m, basket.GrandTotal);
        Assert.Equal(3.0, basket.DistanceKm);
        Assert.Equal("Iki street 3", basket.LocationAddress);
        Assert.Equal(1, _distances.Calls);
    }

    // The created basket is saved once, and opening it by its id gives the same store, totals, trip and rows.
    [Fact]
    public async Task CreatedBasket_IsSaved_AndOpensTheSameById()
    {
        var created = await CreateOk(MilkAndBread(UserLatitude, UserLongitude));

        var result = await _controller.Get(created.Id);
        var opened = Assert.IsType<BasketResponseDto>(Assert.IsType<OkObjectResult>(result).Value);

        Assert.Equal(1, _context.Baskets.Count());
        Assert.Equal(created.StoreName, opened.StoreName);
        Assert.Equal(created.GrandTotal, opened.GrandTotal);
        Assert.Equal(created.DistanceKm, opened.DistanceKm);
        Assert.Equal(created.LocationAddress, opened.LocationAddress);
        Assert.Equal(new List<int> { Milk, Bread }, opened.Rows.Select(r => r.ItemId));
        Assert.Equal(new List<decimal> { 11.00m, 11.00m }, opened.Rows.Select(r => r.RowTotal));
        Assert.Equal(new List<int> { Milk, Bread }, opened.RequestItems.Select(r => r.ItemId));
    }

    // An id that was never saved gives 404.
    [Fact]
    public async Task UnknownBasketId_Gives404()
    {
        var result = await _controller.Get(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    // Kefir exists as an item but no store sells it, so no store can fill milk + kefir: 422, and nothing is saved.
    [Fact]
    public async Task NoStoreHasAllItems_Gives422_AndSavesNothing()
    {
        var items = new List<BasketRequestItemDto> { new(Milk, 1), new(Kefir, 1) };
        var request = new BasketRequestDto(items, "normal", null, UserLatitude, UserLongitude);

        var message = await Create422(request);

        Assert.StartsWith("No store has all these items.", message);
        Assert.Empty(_context.Baskets);
        Assert.Equal(0, _distances.Calls);
    }

    // Max 1 km: Iki's shop (1.1 km) is out by straight line. Max 2 km: it passes, but its road is 3 km, so it is out after Google.
    [Fact]
    public async Task NoStoreWithinMaxKm_Gives422_ByStraightLineAndByRoad()
    {
        var tooFarByStraightLine = await Create422(MilkAndBread(UserLatitude, UserLongitude, 1));

        Assert.Equal("No store with all these items is within 1 km.", tooFarByStraightLine);
        Assert.Equal(0, _distances.Calls);

        var tooFarByRoad = await Create422(MilkAndBread(UserLatitude, UserLongitude, 2));

        Assert.Equal("No store with all these items is within 2 km.", tooFarByRoad);
        Assert.Equal(1, _distances.Calls);
        Assert.Empty(_context.Baskets);
    }

    // Google does not answer: 503 instead of a crash, and nothing is saved.
    [Fact]
    public async Task GoogleFails_Gives503_AndSavesNothing()
    {
        _distances.Fails = true;

        var result = await _controller.Create(MilkAndBread(UserLatitude, UserLongitude));

        var unavailable = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, unavailable.StatusCode);
        Assert.Empty(_context.Baskets);
    }

    // Quantity 0 and an item id that is not in the database give 400 with both problems, before any basket is built.
    [Fact]
    public async Task BadQuantityAndUnknownItem_Give400WithBothProblems()
    {
        var items = new List<BasketRequestItemDto> { new(Milk, 0), new(999, 1) };

        var result = await _controller.Create(new BasketRequestDto(items, "normal"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var problems = Assert.IsType<List<string>>(badRequest.Value);
        Assert.Equal(new List<string> { "Item 1: quantity must be at least 1.", "Item 999 does not exist." }, problems);
    }
}
