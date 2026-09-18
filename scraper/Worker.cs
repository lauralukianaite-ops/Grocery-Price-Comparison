using System.Text.Json;
using Microsoft.Playwright;

namespace scraper;

public class BarboraProductDto
{
    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }
}

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Siunčiama užklausa į Barbora...");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();

            // 1. opens site and takes HTML answer
            var response = await page.GotoAsync("https://barbora.lt/paieska?q=pienas", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            var html = await response!.TextAsync();

            // 2. cuts JSON between '[' and ']'
            int start = html.IndexOf("window.b_productList = [") + "window.b_productList = ".Length;
            int end = html.IndexOf("];", start) + 1;
            var jsonText = html[start..end];

            // 3. takes only title and price
            using var doc = JsonDocument.Parse(jsonText);
            var products = doc.RootElement.EnumerateArray().Select(p => new BarboraProductDto
            {
                Title = p.GetProperty("title").GetString()!,
                Price = p.GetProperty("price").GetDecimal()
            }).ToList();

            foreach (var product in products)
            {
            logger.LogInformation("Product: {Title} | Price: {Price} €", product.Title, product.Price);
            }
            
            logger.LogInformation("Found products: {count}", products.Count);


            await Task.Delay(1000000, stoppingToken);
        }
    }
}
