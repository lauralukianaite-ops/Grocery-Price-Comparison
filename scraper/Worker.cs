namespace scrapper;

using backend.Data;
using Microsoft.EntityFrameworkCore;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var storeCount = await db.Stores.CountAsync(stoppingToken);
                logger.LogInformation("Worker running at {Time}, {Count} stores in DB", DateTimeOffset.Now, storeCount);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}