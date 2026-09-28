using backend.Entities;

namespace scraper;

public interface IScraper
{
    string StoreName { get; }
    
    IAsyncEnumerable<Item> ScrapeProductsAsync(CancellationToken cancellationToken = default);
}

