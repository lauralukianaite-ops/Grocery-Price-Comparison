using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace scraper;


public class Worker(ILogger<Worker> logger, IEnumerable<IScraper> scrapers) : BackgroundService

{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. goes through all scrapers
            foreach (var scraper in scrapers)
            {
                logger.LogInformation("Opening {Store}...", scraper.StoreName);

                try
                {
                    int count = 0;

                    // Receives data stream
                    await foreach (var product in scraper.ScrapeProductsAsync(stoppingToken))
                    {
                        count++;
                        logger.LogInformation("[{Store}] {Title} | {Price} €", scraper.StoreName, product.Title, product.Price);

                        // HERE save to base!!!!!
                    }

                    logger.LogInformation("{Store} found: {count}", scraper.StoreName, count);                
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Klaida renkant duomenis iš {Store}", scraper.StoreName);
                }
            }
            await Task.Delay(1000000, stoppingToken);

        }
    }
}