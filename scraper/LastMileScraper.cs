using System.Runtime.CompilerServices;
using System.Text.Json;
using backend.Entities;
using Microsoft.Playwright;

namespace scraper;

public class LastMileScraper : IScraper
{
    public string StoreName => "Iki";
    List<string> Urls = ["https://www.lastmile.lt/chain/IKI/categories/Vaisiai-ir-uogos",
                        "https://www.lastmile.lt/chain/IKI/categories/Darzoves-ir-grybai",
                        "https://www.lastmile.lt/chain/IKI/categories/Pienas",
                        "https://www.lastmile.lt/chain/IKI/categories/Sviestas-margarinas-riebalai",
                        "https://www.lastmile.lt/chain/IKI/categories/Suris",
                        "https://www.lastmile.lt/chain/IKI/categories/Varskes-produktai",
                        "https://www.lastmile.lt/chain/IKI/categories/Grietine-ir-grietinele",
                        "https://www.lastmile.lt/chain/IKI/categories/Jogurtai-ir-desertai",
                        "https://www.lastmile.lt/chain/IKI/categories/Kiausiniai",
                        "https://www.lastmile.lt/chain/IKI/categories/Kefyras-rugpienis-pasukos",
                        "https://www.lastmile.lt/chain/IKI/categories/Majonezas-ir-padazai",
                        "https://www.lastmile.lt/chain/IKI/categories/Augaliniai-ir-be-laktozes-produktai",
                        "https://www.lastmile.lt/chain/IKI/categories/Svieziai-kepti-duonos-gaminiai",
                        "https://www.lastmile.lt/chain/IKI/categories/Duona",
                        "https://www.lastmile.lt/chain/IKI/categories/Duonos-pakaitalai",
                        "https://www.lastmile.lt/chain/IKI/categories/Dziuvesiai-meduoliai-riestainiai-ir-javinukai",
                        "https://www.lastmile.lt/chain/IKI/categories/Bandeles-spurgos",
                        "https://www.lastmile.lt/chain/IKI/categories/Konditerija",
                        "https://www.lastmile.lt/chain/IKI/categories/Sviezia-mesa",
                        "https://www.lastmile.lt/chain/IKI/categories/Sviezia-paukstiena",
                        "https://www.lastmile.lt/chain/IKI/categories/Mesos-ir-paukstienos-gaminiai",
                        "https://www.lastmile.lt/chain/IKI/categories/Marinuota-mesa-ir-paukstiena",
                        "https://www.lastmile.lt/chain/IKI/categories/Sviezia-zuvis-ir-juru-gerybes",
                        "https://www.lastmile.lt/chain/IKI/categories/Zuvies-gaminiai",
                        "https://www.lastmile.lt/chain/IKI/categories/Kulinarija",
                        "https://www.lastmile.lt/chain/IKI/categories/Augaliniai-mesos-pakaitalai",
                        "https://www.lastmile.lt/chain/IKI/categories/Meksikos-virtuve",
                        "https://www.lastmile.lt/chain/IKI/categories/Japonijos-virtuve",
                        "https://www.lastmile.lt/chain/IKI/categories/Kinijos-virtuve",
                        "https://www.lastmile.lt/chain/IKI/categories/Saldytos-darzoves-uogos-vaisiai-ir-grybai",
                        "https://www.lastmile.lt/chain/IKI/categories/Ledai-ir-ledo-kubeliai",
                        "https://www.lastmile.lt/chain/IKI/categories/Saldyti-kulinarijos-ir-konditerijos-gaminiai",
                        "https://www.lastmile.lt/chain/IKI/categories/Saldyta-zuvis-ir-juru-gerybes",
                        "https://www.lastmile.lt/chain/IKI/categories/Kava-kakava-kavos-gerimai",
                        "https://www.lastmile.lt/chain/IKI/categories/Marinuotas-konservuotas-maistas",
                        "https://www.lastmile.lt/chain/IKI/categories/Aliejus-ir-actas",
                        "https://www.lastmile.lt/chain/IKI/categories/Cukrus-saldikliai-medus",
                        "https://www.lastmile.lt/chain/IKI/categories/Arbata",
                        "https://www.lastmile.lt/chain/IKI/categories/Makaronai",
                        "https://www.lastmile.lt/chain/IKI/categories/Miltai",
                        "https://www.lastmile.lt/chain/IKI/categories/Kruopos",
                        "https://www.lastmile.lt/chain/IKI/categories/Ankstines-darzoves",
                        "https://www.lastmile.lt/chain/IKI/categories/Padazai-ir-uztepeles",
                        "https://www.lastmile.lt/chain/IKI/categories/Sausi-pusryciai-dribsniai-ir-javainiu-batoneliai",
                        "https://www.lastmile.lt/chain/IKI/categories/Riesutai-seklos-dziovinti-vaisiai-uogos",
                        "https://www.lastmile.lt/chain/IKI/categories/Greitai-paruosiamas-maistas",
                        "https://www.lastmile.lt/chain/IKI/categories/Maisto-ruosimo-priedai",
                        "https://www.lastmile.lt/chain/IKI/categories/Prieskoniai",
                        "https://www.lastmile.lt/chain/IKI/categories/Gaivieji-gerimai",
                        "https://www.lastmile.lt/chain/IKI/categories/Vanduo",
                        "https://www.lastmile.lt/chain/IKI/categories/Sultys-nektarai-ir-sulciu-gerimai",
                        "https://www.lastmile.lt/chain/IKI/categories/Energiniai-gerimai",
                        "https://www.lastmile.lt/chain/IKI/categories/Saldumynai",
                        "https://www.lastmile.lt/chain/IKI/categories/Sausainiai-ir-vafliai",
                        "https://www.lastmile.lt/chain/IKI/categories/Uzkandziai",
                        "https://www.lastmile.lt/chain/IKI/categories/Alus",
                        "https://www.lastmile.lt/chain/IKI/categories/Vynas-ir-sampanas",
                        "https://www.lastmile.lt/chain/IKI/categories/Sidras-ir-kokteiliai",
                        "https://www.lastmile.lt/chain/IKI/categories/Stiprieji-alkoholiniai-gerimai",
                        "https://www.lastmile.lt/chain/IKI/categories/Nealkoholiniai-gerimai",
                        "https://www.lastmile.lt/chain/IKI/categories/Baltyminiai-batoneliai",
                        "https://www.lastmile.lt/chain/IKI/categories/Vitaminai-ir-mineralai",
                        "https://www.lastmile.lt/chain/IKI/categories/Kiti-papildai"];

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

        foreach(string url in Urls)
        {
            // 2. before openning the url, says that "frontend-products" is expected
            var responseTask = page.WaitForResponseAsync(r => r.Url.Contains("frontend-products") && r.Status == 200);
            await page.GotoAsync(url);
            
            // 3. scrapes starting page items
            var response = await responseTask;
            foreach (var item in await ExtractItemsAsync(response))
            {
                yield return item;
            }

            while (true)
            {
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
                // 8. sends found products to worker
                foreach (var item in newItems)
                {
                    yield return item;
                }

                await page.WaitForTimeoutAsync(rand.Next(1000, 2000));
            }
            await page.WaitForTimeoutAsync(rand.Next(1000, 2000));
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
        
}