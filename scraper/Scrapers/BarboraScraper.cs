using System.Runtime.CompilerServices;
using System.Text.Json;
using backend.Entities;
using Microsoft.Playwright;

namespace scraper;

public class BarboraScraper : IScraper
{
    private readonly IStoreConfigLoader _configLoader;
    private readonly String _configFilePath;

    public string StoreName { get; private set;} = string.Empty;
    private Dictionary<string, List<string>> AllStores = new();

    // constructor opens and reads all of the "LastMileStores.json" provided store information
    public BarboraScraper(IStoreConfigLoader configLoader, 
                            string configFilePath = "Data/BarboraStores.json")
    {
        _configLoader = configLoader;
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
                    var response = await page.GotoAsync(url + $"?page={pageUrl}", new PageGotoOptions
                    {
                    WaitUntil = WaitUntilState.DOMContentLoaded
                    });
                    if (response == null)
                    {
                        pageUrl++;
                        continue;
                    } 

                    var html = await response.TextAsync();

                    // 2. finds window.b_productList and cuts it and converts to JSON         
                    int markerIndex = html.IndexOf("window.b_productList = [");
                    if (markerIndex == -1){
                        pageUrl++;
                        continue;
                    } 

                    int start = markerIndex + "window.b_productList = ".Length;
                    int end = html.IndexOf("];", start);
                    if (end == -1 || end <= start){
                        pageUrl++;
                        continue;
                    } 

                    var jsonText = html[start..(end + 1)];

                    // 3. takes jsonText
                    var json = JsonSerializer.Deserialize<JsonElement>(jsonText);

                    // 4. scrapes item information
                    var items = json.EnumerateArray().Select(p => new Item
                    {
                        Name = p.GetProperty("title").GetString()!,
                        Prices = new List<Price>
                        {
                            new Price
                            {
                                Cost = p.GetProperty("price").GetDecimal(),
                                RetailCost = p.GetNullableDecimal("retail_price"),
                                RecordedAt = DateTime.UtcNow
                            }
                        }
                    }).ToList();

                    // 5. if page is empty, goes to next url.
                    if (items.Count == 0)break;

                    // 6. sends the information of items to worker
                    foreach (var item in items)
                    {
                        yield return item;
                    }
                    
                    pageUrl++;
                    await Task.Delay(rand.Next(2000, 3500), cancellationToken);
                }
            }
        }
    }

    public async Task InitializeAsync()
    {
        AllStores = await _configLoader.LoadStoresAsync(_configFilePath);
        StoreName = AllStores.First().Key;
    }
}