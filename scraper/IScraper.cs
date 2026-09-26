namespace scraper;

public interface IScraper
{
    string StoreName { get; }
    
    IAsyncEnumerable<ProductData> ScrapeProductsAsync(CancellationToken cancellationToken = default);
}

