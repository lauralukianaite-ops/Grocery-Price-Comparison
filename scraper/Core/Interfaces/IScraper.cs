using backend.Entities;
using Microsoft.Playwright;

namespace scraper;

public interface IScraper
{
    string StoreName { get;}
    
    IAsyncEnumerable<Item> ScrapeProductsAsync(CancellationToken cancellationToken = default);

    Task InitializeAsync();
}

