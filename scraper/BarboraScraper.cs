using System.Text.Json;
using Microsoft.Playwright;

namespace scraper;

public class BarboraScraper : IScraper
{
    public string StoreName => "Barbora";

    public async Task<List<ProductData>> ScrapeProductsAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        var page = await browser.NewPageAsync();

        // 1. opens site and takes HTML answer
        
        //var response = await page.GotoAsync($"https://barbora.lt/paieska?q={searchQuery}", new PageGotoOptions
        var response = await page.GotoAsync($"https://barbora.lt/gera-kaina", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        if (response == null) return new List<ProductData>();

        var html = await response.TextAsync();

        // 2. cuts JSON between '[' and ']'
        // for product list "window.b_productList", for single product "window.product"
        int start = html.IndexOf("window.b_productList = [") + "window.b_productList = ".Length;
        int end = html.IndexOf("];", start) + 1;
        var jsonText = html[start..end];

        // 3. takes only title and price
        using var doc = JsonDocument.Parse(jsonText);
        return doc.RootElement.EnumerateArray().Select(p => new ProductData
        {
            Title = p.GetProperty("title").GetString()!,
            Price = p.GetProperty("price").GetDecimal()
        }).ToList();
    }
}