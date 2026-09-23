namespace scraper;

public interface IScraper
{
    string StoreName { get; }
    Task<List<ProductData>> ScrapeProductsAsync(CancellationToken cancellationToken = default);
}