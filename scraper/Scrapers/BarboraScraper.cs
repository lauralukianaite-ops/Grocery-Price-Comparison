using System.Runtime.CompilerServices;
using System.Text.Json;
using backend.Entities;
using Microsoft.Playwright;

namespace scraper;

public class BarboraScraper : IScraper
{
    private readonly IStoreConfigLoader _configLoader;
    private readonly ILogger<BarboraScraper> _logger;
    private readonly string _configFilePath;

    public string StoreName { get; private set;} = string.Empty;
    private Dictionary<string, List<string>> AllStores = new();

    // constructor opens and reads all of the "BarboraStores.json" provided store information
    public BarboraScraper(IStoreConfigLoader configLoader,
                            ILogger<BarboraScraper> logger,
                            string configFilePath = "Data/BarboraStores.json")
    {
        _configLoader = configLoader;
        _logger = logger;
        _configFilePath = configFilePath;
    }

    public async IAsyncEnumerable<Item> ScrapeProductsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() 
        {
            Headless = false,
            Args = new[] 
            { 
                "--disable-blink-features=AutomationControlled",
                "--start-maximized" 
            }
        });
        var page = await browser.NewPageAsync();
        await page.AddInitScriptAsync("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

        Random rand = new Random();
        foreach(var storeEntry in AllStores)
        {
            StoreName = storeEntry.Key;
            var urls = storeEntry.Value;

            foreach(string url in urls)
            {
                int pageUrl = 1;

                // breaks when empty page of an url is reached
                while (true)
                {
                    // 1. opens site and takes HTML answer
                    IResponse? response;
                    try
                    {
                        response = await page.GotoAsync(
                            $"{url}?page={pageUrl}",
                            new() { WaitUntil = WaitUntilState.DOMContentLoaded });
                    }
                    catch (TimeoutException ex)
                    {
                        _logger.LogWarning(ex, "Timed out navigating to Barbora page {Page} at {Url}; moving to the next category", pageUrl, url);
                        break;
                    }

                    if (response == null)
                    {
                        _logger.LogWarning("No response received for Barbora page {Page} at {Url}; moving to the next category", pageUrl, url);
                        break;
                    }

                    if (response.Status != 200)
                    {
                        _logger.LogWarning("Barbora page {Page} at {Url} returned HTTP {Status}; moving to the next category", pageUrl, url, response.Status);
                        break;
                    }

                    var items = await ExtractItemsAsync(response);
                    if (items.Count == 0)break;

                    // 2. sends the information of items to worker
                    foreach (var item in items)
                    {
                        yield return item;
                    }
                    
                    pageUrl++;
                    await Task.Delay(rand.Next(500, 1000), cancellationToken);
                }
            }
        }
    }

    private async Task<List<Item>> ExtractItemsAsync(IResponse response)
    {
        var html = await response.TextAsync();

        // 1. finds inventories and cuts it and converts to JSON         
        string marker = "\"inventories\"";
        int markerIndex = html.IndexOf(marker);
        if (markerIndex == -1)
        {
            return new List<Item>();
        } 

        int start = html.IndexOf('[', markerIndex); // starts from '['
        if (start == -1)
        {
            return new List<Item>();
        }

        // 2. lets Utf8JsonReader find the end of the complete inventories array
        var bytes = System.Text.Encoding.UTF8.GetBytes(html[start..]);
        var reader = new Utf8JsonReader(bytes);
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray || !reader.TrySkip())
        {
            return new List<Item>();
        }

        var jsonText = System.Text.Encoding.UTF8.GetString(bytes.AsSpan(0, (int)reader.BytesConsumed));
        var json = JsonSerializer.Deserialize<JsonElement>(jsonText);

        // 3. scrapes item information only when it is in stock (the status is "active")
        return json.EnumerateArray()
            .Where(p => p.GetProperty("status").GetString() == "active")
            .Select(p => new Item
        {
            Name = p.GetProperty("title").GetString()!,
            Prices = new List<Price>
            {
                new Price
                {
                    Cost = p.GetProperty("price").GetDecimal(),
                    RetailCost = GetRetailCost(p),
                    RecordedAt = DateTime.UtcNow
                }
            }
        }).ToList();
    }

    private static decimal? GetRetailCost(JsonElement product)
    {
        if (!product.TryGetProperty("promotion", out var promo) || promo.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return promo.GetNullableDecimal("oldPrice");
    }

    public async Task InitializeAsync()
    {
        AllStores = await _configLoader.LoadStoresAsync(_configFilePath);
        StoreName = AllStores.First().Key;
    }
}