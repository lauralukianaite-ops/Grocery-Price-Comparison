using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using backend.Data;
using backend.Repositories;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace backend.Tests;

public class ItemSimilarityIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public ItemSimilarityIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
public async Task GetSimilarItemsAsync_MeasuresRealDbAndAlgorithmTime()
{
    var connectionString = GetConnectionString();

    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseNpgsql(connectionString);

    await using var dbContext = new AppDbContext(optionsBuilder.Options);
    var repository = new ItemsRepository(dbContext);

    //DB loading time
    var dbSw = Stopwatch.StartNew();
    var itemsFromDb = await repository.GetItemsWithPriceAndStoreAsync();
    dbSw.Stop();

    //Creating in-memory index
    var indexSw = Stopwatch.StartNew();
    var index = new ItemSimilarityIndex();
    index.BuildOrUpdateIndex(itemsFromDb);
    indexSw.Stop();

    var service = new ItemSimilarityService(index);

    var searchSw = Stopwatch.StartNew();
    var response = await service.GetSimilarItemsAsync("Pienas", 0.3);
    searchSw.Stop();

    _output.WriteLine($"[INTEGRATION TEST] Products in DB: {itemsFromDb.Count}");
    _output.WriteLine($"[INTEGRATION TEST] DB loading time: {dbSw.ElapsedMilliseconds} ms");
    _output.WriteLine($"[INTEGRATION TEST] In-memory Index creation time: {indexSw.ElapsedMilliseconds} ms");
    _output.WriteLine($"[INTEGRATION TEST] Search from RAM: {searchSw.ElapsedMilliseconds} ms");

    Assert.NotNull(response);
    Assert.NotEmpty(response.SimilarItems);
}

private static string GetConnectionString()
{
    var envPath = Path.Combine(AppContext.BaseDirectory, ".env");

    if (File.Exists(envPath))
    {
        DotNetEnv.Env.Load(envPath);
    }
    else
    {
        DotNetEnv.Env.Load();
    }

    var host = Environment.GetEnvironmentVariable("GlobalSettings__DatabaseHost");
    var port = Environment.GetEnvironmentVariable("GlobalSettings__DatabasePort");
    var db = Environment.GetEnvironmentVariable("GlobalSettings__DatabaseName");
    var user = Environment.GetEnvironmentVariable("GlobalSettings__DatabaseUser");
    var pass = Environment.GetEnvironmentVariable("GlobalSettings__DatabasePassword");

    if (!string.IsNullOrEmpty(host))
    {
        return $"Host={host};Port={port};Database={db};Username={user};Password={pass};SslMode=Require;";
    }

    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(dbUrl))
        {
            return dbUrl;
        }

        throw new InvalidOperationException("Failed to load database from .env file.");
    }
}