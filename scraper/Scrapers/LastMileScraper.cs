using System.Runtime.CompilerServices;
using System.Text.Json;
using backend.Entities;
using Microsoft.Playwright;

namespace scraper;

public class LastMileScraper : IScraper
{
    private readonly IStoreConfigLoader _configLoader;
    private readonly String _configFilePath;

    public string StoreName { get; private set;} = string.Empty;
    private Dictionary<string, List<string>> AllStores = new();

    // constructor opens and reads all of the "LastMileStores.json" provided store information
    public LastMileScraper(IStoreConfigLoader configLoader, 
                            string configFilePath = "Data/LastMileStores.json")
    {
        _configLoader = configLoader;
        _configFilePath = configFilePath;
    }

    public async IAsyncEnumerable<Item> ScrapeProductsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 1. runs Playwright with anti-detection arguments
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
                // 2. before openning the url, says that "frontend-products" is expected
                var responseTask = page.WaitForResponseAsync(r => r.Url.Contains("frontend-products") && r.Status == 200);
                await page.GotoAsync(url);
                
                // 3. scrapes starting page items
                var response = await responseTask;
                if (response == null) continue;
                foreach (var item in await ExtractItemsAsync(response))
                {
                    yield return item;
                }

                while (true)
                {
                    // accepts cookies
                    try
                    {
                        var acceptButton = page.Locator("button:has-text('Priimti visus')");
                        
                        // waits until the button is visable
                        await acceptButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
                        
                        // clicks it
                        await acceptButton.ClickAsync();
                    }
                    catch
                    {
                        // if there was no cookies request
                    }

                    // 4. locates the "Rodyti daugiau" button by its visible text
                    var loadMoreButton = page.Locator("button:has-text('Rodyti daugiau')");

                    // breaks if button is not visible
                    if (!await loadMoreButton.IsVisibleAsync())
                    {
                        break;
                    }

                    // 5. before pressing the button, says that "frontend-products" is expected
                    var nextResponseTask = page.WaitForResponseAsync(r => r.Url.Contains("frontend-products") && r.Status == 200);

                    // 6. presses the button
                    await loadMoreButton.ClickAsync();

                    // 7. waits for the response and scrapes it
                    var nextResponse = await nextResponseTask;
                    var newItems = await ExtractItemsAsync(nextResponse);

                    // 8. breaks if no products were found
                    if (newItems.Count == 0)
                    {
                        break;
                    }
                    // 9. sends found products to worker
                    foreach (var item in newItems)
                    {
                        yield return item;
                    }

                    await page.WaitForTimeoutAsync(rand.Next(1000, 2000));
                }
                await page.WaitForTimeoutAsync(rand.Next(1000, 2000));
            }
        }
    }

    private async Task<List<Item>> ExtractItemsAsync(IResponse response)
    {
        var json = await response.JsonAsync();

        // 1. checks if json is null and does it have any products
        if (json is null 
            || !json.Value.TryGetProperty("products", out var productsProp) // takes all of the products
            || productsProp.ValueKind != JsonValueKind.Array)
        {
            return new List<Item>();
        }
            
        // 2. goes trhough the products and puts them to a list
        return productsProp.EnumerateArray().Select(p =>
        {
            var fp = p.GetProperty("frontEndProduct");
            var costPrice = fp.GetProperty("costPrice");

            // cost without discounts
            decimal pCost = costPrice.GetProperty("p").GetDecimal();
            // cost with the highest discount
            decimal? lCost = costPrice.GetNullableDecimal("l");

            // 3. correctly places Cost and RetailCost
            decimal finalCost;
            decimal? retailCost;
            if(lCost is null)
            {
                finalCost = pCost;
                retailCost = null;
            }
            else
            {
                finalCost = (decimal)lCost;
                retailCost = pCost;
            }

            // 4. creates the item
            return new Item
            {
                Name = fp.GetProperty("name").GetProperty("lt").GetString()!,
                Prices = new List<Price>
                {
                    new Price
                    {
                        Cost = finalCost,
                        RetailCost = retailCost,
                        RecordedAt = DateTime.UtcNow
                    }
                }
            };
        }).ToList();
    }

    public async Task InitializeAsync()
    {
        AllStores = await _configLoader.LoadStoresAsync(_configFilePath);
        StoreName = AllStores.First().Key;
    }
}