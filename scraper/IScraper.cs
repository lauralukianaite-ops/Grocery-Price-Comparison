namespace scraper;

public interface IScraper
{
    string StoreName { get; }
    Task<List<ProductData>> ScrapeProductsAsync(string searchQuery, CancellationToken cancellationToken = default);
}