using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace scraper;


public class Worker(ILogger<Worker> logger, 
    IEnumerable<IScraper> scrapers,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // goes through all scrapers
            foreach (var scraper in scrapers)
            {
                logger.LogInformation("Opening {Store}...", scraper.StoreName);

                try
                {
                    // 1. creates scope for DB operations
                    using var scope = scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // 2. finds or creates a store
                    var store = await dbContext.Stores.FirstOrDefaultAsync(s => s.Name == scraper.StoreName, stoppingToken);
                    if (store == null)
                    {
                        store = new Store { Name = scraper.StoreName };
                        dbContext.Stores.Add(store);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }

                    int count = 0;
                    // Receives data stream
                    await foreach (var scrapedItem in scraper.ScrapeProductsAsync(stoppingToken))
                    {
                        count++;

                        // 3. checks if the item exists in DB
                        var existingItem = await dbContext.Items
                            .FirstOrDefaultAsync(i => i.Name == scrapedItem.Name, stoppingToken);

                        var scrapedPrice = scrapedItem.Prices.First();
                        scrapedPrice.StoreId = store.Id;

                        if (existingItem != null)
                        {
                            // new price record added
                            scrapedPrice.ItemId = existingItem.Id;
                            dbContext.Prices.Add(scrapedPrice);
                        }
                        else
                        {
                            // if there is no such item, whole item gets added to DB
                            dbContext.Items.Add(scrapedItem);
                        }
                        
                        // saves after every 20 items
                        if (count % 20 == 0)
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                            dbContext.ChangeTracker.Clear();
                        }

                        logger.LogInformation("[{Store}] {Title} | {Price} € (Retail: {RetailPrice} €)", 
                            scraper.StoreName, 
                            scrapedItem.Name, 
                            scrapedPrice.Cost, 
                            scrapedPrice.RetailCost ?? 0);
                    }
                    // saves any remaining items (ex. scraper finds 21 items so 1 remains unsaved)
                    await dbContext.SaveChangesAsync(stoppingToken);

                    logger.LogInformation("{Store} found: {count}", scraper.StoreName, count);                
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error collecting data {Store}", scraper.StoreName);
                }
            }
            await Task.Delay(1000000, stoppingToken);
        }
    }  
}