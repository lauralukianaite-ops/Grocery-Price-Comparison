using System.Text.Json;
using Microsoft.Playwright;

namespace scraper;

public class BarboraScraper : IScraper
{
    public string StoreName => "Barbora";
    List<string> Urls = ["https://barbora.lt/darzoves-ir-vaisiai",
                        "https://barbora.lt/pieno-gaminiai-kiausiniai-ir-majonezas", 
                        "https://barbora.lt/duonos-gaminiai-ir-konditerija",
                        "https://barbora.lt/mesa-zuvis-ir-kulinarija",
                        "https://barbora.lt/bakaleja",
                        "https://barbora.lt/saldytas-maistas",
                        "https://barbora.lt/gerimai",
                        "https://barbora.lt/kudikiu-ir-vaiku-prekes",
                        "https://barbora.lt/kosmetika-ir-higiena",
                        "https://barbora.lt/svaros-ir-gyvunu-prekes",
                        "https://barbora.lt/namai-ir-laisvalaikis"];

    public async Task<List<ProductData>> ScrapeProductsAsync(CancellationToken cancellationToken = default)
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
        List<ProductData> allProducts = new List<ProductData>();

        foreach(string url in Urls)
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
                if (response == null) continue;

                var html = await response.TextAsync();

                // 2. cuts JSON between '[' and ']'            
                int markerIndex = html.IndexOf("window.b_productList = [");
                if (markerIndex == -1){continue;}

                int start = markerIndex + "window.b_productList = ".Length;
                int end = html.IndexOf("];", start);
                if (end == -1 || end <= start){continue;}

                var jsonText = html[start..(end + 1)];

                // 3. takes jsonText
                using var doc = JsonDocument.Parse(jsonText);

                // 4. takes only title and price
                var products = doc.RootElement.EnumerateArray().Select(p => new ProductData
                {
                Title = p.GetProperty("title").GetString()!,
                Price = p.GetProperty("price").GetDecimal()
                }).ToList();

                // 5. if page is empty, goes to next url.
                if (products.Count == 0)break;

                allProducts.AddRange(products);
                pageUrl++;

                // pause
                await Task.Delay(rand.Next(2000, 3500), cancellationToken);
            }
        }
        return allProducts;
    }
}