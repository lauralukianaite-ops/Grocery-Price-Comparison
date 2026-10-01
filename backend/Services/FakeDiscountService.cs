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
        var result = new List<DiscountFlagResponseDto>();

        foreach (var storePrices in prices.GroupBy(p => p.StoreId))
        {
            var daily = storePrices // one price per store per day, newest day first
                .GroupBy(p => p.RecordedAt.Date)
                .Select(g => g.OrderByDescending(p => p.RecordedAt).First())
                .OrderByDescending(p => p.RecordedAt.Date)
                .ToList();

            if (daily.Count == 0)
                continue;

            var current = daily[0];
            if ((today - current.RecordedAt.Date).TotalDays > 2)
                continue;

            if (current.RetailCost is null || current.RetailCost <= current.Cost)
                continue;

            var retail = current.RetailCost.Value; // won't be null
            var expected = current.RecordedAt.Date;
            var index = 0;

            DiscountVerdict? verdict = null;
            decimal? typicalCost = null;

            // phase 1: walk back to the start of current discount.These days don't count. Deepening discounts are ok
            var campaignDays = 0;
            while (index < daily.Count && daily[index].RecordedAt.Date == expected)
            {
                var day = daily[index];
                if (day.RetailCost != retail || day.Cost < current.Cost)
                    break; // discount start found

                campaignDays++;
                if (campaignDays >= 30)
                {
                    verdict = DiscountVerdict.False;// discount running >30 days is the price
                    break;
                }
                index++;
                expected = expected.AddDays(-1);
            }

            // a same-discount day was cheaper than today = false
            if (verdict is null
                && index < daily.Count
                && daily[index].RecordedAt.Date == expected
                && daily[index].RetailCost == retail
                && daily[index].Cost < current.Cost)
            {
                verdict = DiscountVerdict.False;
            }

            // phase 2: 30 days before the discount started.
            if (verdict is null)
            {
                decimal? lowest = null;
                var daysChecked = 0;

                while (daysChecked < 30
                       && index < daily.Count
                       && daily[index].RecordedAt.Date == expected)
                {
                    var cost = daily[index].Cost;

                    if (cost <= current.Cost)
                    {
                        verdict = DiscountVerdict.False;
                        break;
                    }
                    if (lowest is null || cost < lowest)
                        lowest = cost;

                    index++;
                    expected = expected.AddDays(-1);
                    daysChecked++;
                }

                if (verdict is null)
                {
                    if (daysChecked < 30)
                        continue;

                    verdict = lowest >= retail
                        ? DiscountVerdict.Real 
                        : DiscountVerdict.Exaggerated;
                    typicalCost = lowest;
                }
            }

            result.Add(new DiscountFlagResponseDto(
                current.StoreId,
                current.Store!.Name,
                current.Cost,
                retail,
                typicalCost,
                verdict.Value));
        }

        return result;
    }

}