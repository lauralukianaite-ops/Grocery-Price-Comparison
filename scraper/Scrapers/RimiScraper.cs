using System.Runtime.CompilerServices;
using System.Text.Json;
using backend.Entities;
using Microsoft.Playwright;

namespace scraper;

public class RimiScraper : IScraper
{
    private readonly IStoreConfigLoader _configLoader;
    private readonly String _configFilePath;

    public string StoreName { get; private set;} = string.Empty;
    private Dictionary<string, List<string>> AllStores = new();

    // constructor opens and reads all of the "RimiStores.json" provided store information
    public RimiScraper(IStoreConfigLoader configLoader, 
                            string configFilePath = "Data/RimiStores.json")
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
                    var response = await page.GotoAsync($"{url}?currentPage={pageUrl}&pageSize=80");
                    if (response == null) continue;

                    var items = await ExtractItemsAsync(response);
                    if (items.Count == 0)break;

                    // 2. sends the information of items to worker
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


    private async Task<List<Item>> ExtractItemsAsync(IResponse response)
    {
        var html = await response.TextAsync();
        var items = new List<Item>();

        var cards = html.Split("data-product-code=");

        foreach (var cardHtml in cards.Skip(1))
        {
            try
            {
                if (cardHtml.Contains("Šiuo metu prekės nėra"))continue;

                // 1. finds "data-gtm-eec-product=" and takes name from it
                string jsonMarker = "data-gtm-eec-product='";
                int jsonStart = cardHtml.IndexOf(jsonMarker) + jsonMarker.Length;
                int jsonEnd = cardHtml.IndexOf("'", jsonStart);
                
                var jsonText = System.Net.WebUtility.HtmlDecode(cardHtml[jsonStart..jsonEnd]);
                using var jsonDoc = JsonDocument.Parse(jsonText);
                string name = jsonDoc.RootElement.GetProperty("name").GetString()!;

                // 2. takes regular price from .sr-only
                string srMarker = "<span class=\"sr-only\">";
                int priceStart = cardHtml.IndexOf(srMarker) + srMarker.Length;
                int priceEnd = cardHtml.IndexOf("</span>", priceStart);
                decimal regularPrice = ParsePrice(cardHtml[priceStart..priceEnd]);

                // 3. searches for promo price
                decimal? promoPrice = null;
                int promoIndex = cardHtml.IndexOf("price-label__body");
                if (promoIndex != -1)
                {
                    string unitMarker = "price-per-unit\">";
                    int unitStart = cardHtml.IndexOf(unitMarker, promoIndex);
                    if (unitStart != -1)
                    {
                        unitStart += unitMarker.Length;
                        int unitEnd = cardHtml.IndexOf("</div>", unitStart);
                        string promoText = cardHtml[unitStart..unitEnd];
                        promoPrice = ParsePrice(promoText);
                    }
                }

                items.Add(new Item
                {
                    Name = name,
                    Prices = new List<Price>
                    {
                        new Price 
                        { 
                            Cost = promoPrice ?? regularPrice,
                            RetailCost = promoPrice != null ? regularPrice : null,
                            RecordedAt = DateTime.UtcNow 
                        }
                    }
                });
            }
            catch
            {
                // if card did not have structure, it is passed
            }
        }

        return items;
    }

    private decimal ParsePrice(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+[\.,]?\d*");
        if (match.Success && decimal.TryParse(match.Value.Replace(',', '.'), 
            System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture, out var price))
        {
            return price;
        }
        return 0;
    }

    public async Task InitializeAsync()
    {
        AllStores = await _configLoader.LoadStoresAsync(_configFilePath);
        StoreName = AllStores.First().Key;
    }
}