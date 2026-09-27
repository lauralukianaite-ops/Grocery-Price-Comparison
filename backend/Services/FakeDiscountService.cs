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
            // one price per calendar day, newest day first
            var daily = storePrices
                .GroupBy(p => p.RecordedAt.Date)
                .Select(g => g.OrderByDescending(p => p.RecordedAt).First())
                .OrderByDescending(p => p.RecordedAt.Date)
                .ToList();

            if (daily.Count == 0)
                continue;

            // anchor on the latest scraped day, but only if it's fresh enough
            var current = daily[0];
            if ((today - current.RecordedAt.Date).TotalDays > 2)
                continue;

            // no discount shown in this store
            if (current.RetailCost is null || current.RetailCost <= current.Cost)
                continue;

            var retail = current.RetailCost.Value;
            var expected = current.RecordedAt.Date;
            var index = 0;

            DiscountVerdict? verdict = null;
            decimal? typicalCost = null;

            // phase 1: walk back over the current campaign — consecutive days
            // advertising the same retail price. Deepening steps (higher past
            // cost) are the lawful progressive discount; they don't count
            // against the 30-day window.
            var campaignDays = 0;
            while (index < daily.Count && daily[index].RecordedAt.Date == expected)
            {
                var day = daily[index];
                if (day.RetailCost != retail || day.Cost < current.Cost)
                    break;                          // campaign start found

                campaignDays++;
                if (campaignDays >= 30)
                {
                    verdict = DiscountVerdict.False;    // promo running so long it IS the price
                    break;
                }
                index++;
                expected = expected.AddDays(-1);
            }

            // a same-campaign day cheaper than today: discount got shallower,
            // so today's price already sold lower under the same "was" claim
            if (verdict is null
                && index < daily.Count
                && daily[index].RecordedAt.Date == expected
                && daily[index].RetailCost == retail
                && daily[index].Cost < current.Cost)
            {
                verdict = DiscountVerdict.False;
            }

            // phase 2: 30 days before the campaign started. Every price counts
            // toward the reference minimum, including earlier separate promos.
            if (verdict is null)
            {
                decimal? lowest = null;
                var daysChecked = 0;

                while (daysChecked < 30
                       && index < daily.Count
                       && daily[index].RecordedAt.Date == expected)   // missing day ends the window
                {
                    var cost = daily[index].Cost;

                    if (cost <= current.Cost)
                    {
                        verdict = DiscountVerdict.False;    // sold at/below today's price
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
                        continue;   // window incomplete → nothing provable, no flag

                    verdict = lowest >= retail
                        ? DiscountVerdict.Real          // retail really was the 30-day low
                        : DiscountVerdict.Exaggerated;  // it sold cheaper than the claimed "was"
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