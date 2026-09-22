namespace scraper;

public class Worker(ILogger<Worker> logger, IEnumerable<IScraper> scrapers) : BackgroundService
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
                    var products = await scraper.ScrapeProductsAsync(stoppingToken);

                    foreach (var product in products)
                    {
                        logger.LogInformation("[{Store}] {Title} | {Price} €", scraper.StoreName, product.Title, product.Price);
                    }

                    logger.LogInformation("{Store} found: {count}", scraper.StoreName, products.Count);
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