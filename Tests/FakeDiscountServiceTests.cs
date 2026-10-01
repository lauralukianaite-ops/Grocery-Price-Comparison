using System;
using System.Collections.Generic;
using System.Linq;
using backend.DTOs;
using backend.Entities;
using backend.Services;
using Xunit;

namespace backend.Tests;

public class FakeDiscountServiceTests
{
    private static readonly DateTime Today = new DateTime(2026, 9, 27);

    private static Price P(int daysAgo, decimal cost, decimal? retail = null, int storeId = 1, string store = "Barbora")
    {
        return new Price
        {
            StoreId = storeId,
            ItemId = 1,
            Cost = cost,
            RetailCost = retail,
            RecordedAt = Today.AddDays(-daysAgo),
            Store = new Store { Id = storeId, Name = store }
        };
    }

    private static List<DiscountFlagResponseDto> Run(List<Price> prices)
    {
        return new FakeDiscountService(null).FindDiscountFlags(prices, Today);
    }

    [Fact]
    public void NoDiscountShown_ReturnsNothing()
    {
        var prices = new List<Price> { P(0, 6m), P(0, 6m, 6m, 2), P(0, 6m, 5m, 3) };

        var results = Run(prices);

        Assert.Empty(results);
    }

    [Fact]
    public void DiscountRunningThirtyDays_IsFalse()
    {
        var prices = new List<Price>();
        for (var d = 0; d < 30; d++)
            prices.Add(P(d, 6m, 10m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.False, result.Verdict);
        Assert.Null(result.TypicalCost);
    }

    [Fact]
    public void PriceUnchangedBeforeLabelAppeared_IsFalse()
    {
        var prices = new List<Price> { P(0, 8m, 10m) };
        for (var d = 1; d <= 10; d++)
            prices.Add(P(d, 8m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.False, result.Verdict);
    }

    [Fact]
    public void DiscountGotShallower_IsFalse()
    {
        var prices = new List<Price> { P(0, 8m, 10m), P(1, 7m, 10m) };

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.False, result.Verdict);
    }

    [Fact]
    public void LowerPriceFoundBeforeGap_IsStillFalse()
    {
        var prices = new List<Price> { P(0, 6m, 10m), P(1, 8m), P(2, 5m) };

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.False, result.Verdict);
        Assert.Null(result.TypicalCost);
    }

    [Fact]
    public void RetailSeenButCheaperNormalPriceExists_IsExaggerated()
    {
        var prices = new List<Price> { P(0, 6m, 10m) };
        for (var d = 1; d <= 20; d++)
            prices.Add(P(d, 8m));
        for (var d = 21; d <= 30; d++)
            prices.Add(P(d, 10m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.Exaggerated, result.Verdict);
        Assert.Equal(8m, result.TypicalCost);
    }

    [Fact]
    public void RetailIsThirtyDayLow_IsReal()
    {
        var prices = new List<Price> { P(0, 6m, 10m) };
        for (var d = 1; d <= 30; d++)
            prices.Add(P(d, 10m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.Real, result.Verdict);
        Assert.Equal(10m, result.TypicalCost);
    }

    [Fact]
    public void ProgressiveDeepeningDiscount_IsReal()
    {
        var prices = new List<Price> { P(0, 5m, 10m) };
        for (var d = 1; d <= 7; d++)
            prices.Add(P(d, 6m, 10m));
        for (var d = 8; d <= 37; d++)
            prices.Add(P(d, 10m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.Real, result.Verdict);
    }

    [Fact]
    public void WindowIncomplete_NoFlag()
    {
        var prices = new List<Price> { P(0, 6m, 10m) };
        for (var d = 1; d <= 5; d++)
            prices.Add(P(d, 8m));

        Assert.Empty(Run(prices));
    }

    [Fact]
    public void TodayNotScraped_AnchorsOnLatestDay()
    {
        var prices = new List<Price> { P(1, 6m, 10m) };
        for (var d = 2; d <= 31; d++)
            prices.Add(P(d, 10m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.Real, result.Verdict);
    }

    [Fact]
    public void DataOlderThanTwoDays_NoFlag()
    {
        var prices = new List<Price> { P(5, 6m, 10m), P(6, 10m) };

        Assert.Empty(Run(prices));
    }

    [Fact]
    public void StoresAreJudgedIndependently()
    {
        var prices = new List<Price>
        {
            P(0, 6m, 10m),
            P(0, 8m, 10m, storeId: 2, store: "Iki"),
            P(1, 8m, storeId: 2, store: "Iki")
        };
        for (var d = 1; d <= 30; d++)
            prices.Add(P(d, 10m));

        var results = Run(prices);

        Assert.Equal(2, results.Count);
        Assert.Equal(DiscountVerdict.Real, results.First(r => r.StoreId == 1).Verdict);
        Assert.Equal(DiscountVerdict.False, results.First(r => r.StoreId == 2).Verdict);
    }
    
    [Fact]
    public void DiscountedPriceAboveEarlierRegularPrice_IsFalse()
    {
        // retail:12 . 8 x20, 12 x10, 9.
        var prices = new List<Price> { P(0, 9m, 12m) };
        for (var d = 1; d <= 10; d++)
            prices.Add(P(d, 12m));
        for (var d = 11; d <= 30; d++)
            prices.Add(P(d, 8m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.False, result.Verdict);
        Assert.Null(result.TypicalCost);
    }

    [Fact]
    public void LowPriceOlderThanWindow_IsReal()
    {
        // retail:7 . 5 x10, 7 x30, 6.
        var prices = new List<Price> { P(0, 6m, 7m) };
        for (var d = 1; d <= 30; d++)
            prices.Add(P(d, 7m));
        for (var d = 31; d <= 40; d++)
            prices.Add(P(d, 5m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.Real, result.Verdict);
        Assert.Equal(7m, result.TypicalCost);
    }

    [Fact]
    public void BlankDateBeyondCompleteWindow_RealStillReturned()
    {
        // retail:10 . 5, _, _, _, _, 10 x30, 6.
        var prices = new List<Price> { P(0, 6m, 10m) };
        for (var d = 1; d <= 30; d++)
            prices.Add(P(d, 10m));
        prices.Add(P(35, 5m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.Real, result.Verdict);
        Assert.Equal(10m, result.TypicalCost);
    }

    [Fact]
    public void BlankDateBeyondCompleteWindow_ExaggeratedStillReturned()
    {
        // retail:10 . 5, _, _, _, _, 10 x15, 8 x15, 6.
        var prices = new List<Price> { P(0, 6m, 10m) };
        for (var d = 1; d <= 15; d++)
            prices.Add(P(d, 8m));
        for (var d = 16; d <= 30; d++)
            prices.Add(P(d, 10m));
        prices.Add(P(35, 5m));

        var result = Assert.Single(Run(prices));
        Assert.Equal(DiscountVerdict.Exaggerated, result.Verdict);
        Assert.Equal(8m, result.TypicalCost);
    }
}