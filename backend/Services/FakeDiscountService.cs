using backend.Data;
using backend.DTOs;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class FakeDiscountService : IFakeDiscountService
{
    private readonly AppDbContext _dbContext;

    public FakeDiscountService(AppDbContext db)
    {
        _dbContext = db;
    }

    public async Task<List<DiscountFlagResponseDto>?> GetDiscountFlag(int itemId)
    {
        var itemExists = await _dbContext.Items.AnyAsync(i => i.Id == itemId);
        if (!itemExists) return null;

        var prices = await _dbContext.Prices
            .Where(p => p.ItemId == itemId)
            .Include(p => p.Store)
            .ToListAsync();

        return FindDiscountFlags(prices, DateTime.UtcNow.Date);
    }

    public List<DiscountFlagResponseDto> FindDiscountFlags(List<Price> prices, DateTime today)
    {
        return prices
            .GroupBy(p => p.StoreId)
            .Select(g => EvaluateStore(ToDaily(g), today))
            .Where(flag => flag is not null)
            .ToList()!;
    }

    // takes a single store's records and gives one price per day, newest day first
    private List<Price> ToDaily(IEnumerable<Price> storePrices)
    {
        return storePrices
            .GroupBy(p => p.RecordedAt.Date)
            .Select(g => g.OrderByDescending(p => p.RecordedAt).First())
            .OrderByDescending(p => p.RecordedAt.Date)
            .ToList();
    }

    private DiscountFlagResponseDto? EvaluateStore(List<Price> daily, DateTime today)
    {
        if (daily.Count == 0) return null; // daily is the full item cost history per store, including gaps

        var current = daily[0]; // last recorded price
        if ((today - current.RecordedAt.Date).TotalDays > 2) return null; // hasnt been scraped in last two days
        if (current.RetailCost is null || current.RetailCost <= current.Cost) return null; // discount not advertised

        // unbroken run of days ending today; a gap in the data ends the history we can use
        var history = daily
            .TakeWhile((p, i) => p.RecordedAt.Date == current.RecordedAt.Date.AddDays(-i))
            .ToList();

        // days the current discount has been running. deepening discounts are ok
        var retail = current.RetailCost.Value;
        var discountDays = history
            .TakeWhile(p => p.RetailCost == retail && p.Cost >= current.Cost)
            .Take(30)
            .Count();

        // the 30 days before the discount started
        var baseline = history.Skip(discountDays).Take(30).ToList();

        DiscountVerdict verdict;
        decimal? typicalCost = null;

        if (discountDays >= 30) verdict = DiscountVerdict.False; // running this long, it's just the price
        else if (baseline.Any(p => p.Cost <= current.Cost)) verdict = DiscountVerdict.False; // was already this cheap before the discount
        else if (baseline.Count < 30) return null; // not enough history to judge
        else
        {
            typicalCost = baseline.Min(p => p.Cost);
            verdict = typicalCost >= retail ? DiscountVerdict.Real : DiscountVerdict.Exaggerated;
        }

        return new DiscountFlagResponseDto(
            current.StoreId,
            current.Store!.Name,
            current.Cost,
            retail,
            typicalCost,
            verdict);
    }
}
