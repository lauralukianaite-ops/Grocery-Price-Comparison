using Microsoft.Playwright;

namespace scraper;

public class BarboraProductDto
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Image { get; set; } = string.Empty;

    public string Brand_Name { get; set; } = string.Empty;
}

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Sending request to Barbora API...");

            // initializes Playwright
            using var playwright = await Playwright.CreateAsync();
            // creates light HTTP client
            var request = await playwright.APIRequest.NewContextAsync();
            // tries to fetch raw data
            var response = await request.GetAsync("https://barbora.lt/paieska?q=pienas");

            if (response.Ok)
            {
                logger.LogInformation("Success {time}", DateTimeOffset.Now);
            }


            
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(10000000, stoppingToken);
        }
    }
}
