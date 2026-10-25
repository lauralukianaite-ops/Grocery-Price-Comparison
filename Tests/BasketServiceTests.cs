using System;
using System.Collections.Generic;
using System.Linq;
using backend.DTOs;
using backend.Entities;
using backend.Enums;
using backend.Services;
using backend.ValueObjects;
using Xunit;

namespace backend.Tests;

public class BasketServiceTests
{
    private static readonly DateTime Today = new DateTime(2026, 9, 27);
    private static readonly SimilarityScore Normal = new(0.3);

    private static readonly BasketService Service =
        new BasketService(null, null, new FakeDiscountService(null), new ItemSimilarityService(null));

    private static Item I(int id, string name)
    {
        return new Item { Id = id, Name = name };
    }

    private static Store S(int id, string name)
    {
        return new Store { Id = id, Name = name };
    }

    private static Price P(Store store, Item item, decimal cost)
    {
        return new Price { StoreId = store.Id, Store = store, ItemId = item.Id, Item = item, Cost = cost };
    }

    private static BasketRequestItem Row(int position, int itemId, int quantity)
    {
        return new BasketRequestItem { Position = position, ItemId = itemId, Quantity = quantity };
    }

    private static BasketRequestDto Request(string strictness, params BasketRequestItemDto[] items)
    {
        return new BasketRequestDto(items.ToList(), strictness);
    }

    // An empty list is the only problem reported, even though "medium" is a bad strictness word too.
    [Fact]
    public void EmptyList_ReportsOnlyThat_EvenWithABadStrictnessWord()
    {
        var emptyListWithBadWord = Request("medium");
        var noExistingItems = new List<int>();

        var problems = Service.FindProblems(emptyListWithBadWord, noExistingItems);

        Assert.Equal(new List<string> { "The list is empty." }, problems);
    }

    // Quantity 0, -3, 1001 and an unknown id 999 in one request give four messages, each naming its item id.
    [Fact]
    public void BadQuantitiesAndUnknownIds_AreAllReportedByItemId()
    {
        var request = Request("normal", new(1, 0), new(2, -3), new(3, 1001), new(999, 1));

        var problems = Service.FindProblems(request, new List<int> { 1, 2, 3 });

        Assert.Equal(4, problems.Count);
        Assert.Contains("Item 1: quantity must be at least 1.", problems);
        Assert.Contains("Item 2: quantity must be at least 1.", problems);
        Assert.Contains("Item 3: quantity must be at most 1000.", problems);
        Assert.Contains("Item 999 does not exist.", problems);
    }

    // Item 5 twice, once with quantity 0: the bad copy is rejected instead of being hidden by merging.
    [Fact]
    public void DuplicateIdWithOneBadQuantity_IsStillRejected()
    {
        var request = Request(null, new(5, 2), new(5, 0));

        var problems = Service.FindProblems(request, new List<int> { 5 });

        Assert.Equal(new List<string> { "Item 5: quantity must be at least 1." }, problems);
    }

    // 101 rows and the word "medium" give two messages in one answer, not one at a time.
    [Fact]
    public void TooManyRowsAndUnknownStrictnessWord_AreBothReported()
    {
        var rows = Enumerable.Range(1, 101).Select(_ => new BasketRequestItemDto(1, 1)).ToList();

        var problems = Service.FindProblems(new BasketRequestDto(rows, "medium"), new List<int> { 1 });

        Assert.Equal(2, problems.Count);
        Assert.Contains("The list has more than 100 rows.", problems);
        Assert.Contains("Strictness must be one of: broad, normal, strict.", problems);
    }

    // [7x2, 3x1, 7x3] becomes [7x5 at position 1, 3x1 at position 2]: summed, first appearance first.
    [Fact]
    public void DuplicateIds_MergeIntoOneEntry_InFirstAppearanceOrder()
    {
        var merged = Service.MergeDuplicates(new List<BasketRequestItemDto> { new(7, 2), new(3, 1), new(7, 3) });

        Assert.Equal(2, merged.Count);
        Assert.Equal(1, merged[0].Position);
        Assert.Equal(7, merged[0].ItemId);
        Assert.Equal(5, merged[0].Quantity);
        Assert.Equal(2, merged[1].Position);
        Assert.Equal(3, merged[1].ItemId);
        Assert.Equal(1, merged[1].Quantity);
    }

    // Gamma has the cheapest milk but no bread, so Beta wins with both.
    [Fact]
    public void StoreMissingOneRow_IsOutEvenIfCheaperElsewhere()
    {
        var milk = I(1, "pienas");
        var bread = I(2, "duona");
        var gamma = S(1, "Gamma");
        var beta = S(2, "Beta");
        var prices = new List<Price> { P(gamma, milk, 0.50m), P(beta, milk, 1.20m), P(beta, bread, 0.50m) };
        var rows = new List<BasketRequestItem> { Row(1, 1, 1), Row(2, 2, 1) };

        var basket = Service.FindCheapestBasket(new List<Item> { milk, bread }, prices, rows, Normal);

        Assert.Equal("Beta", basket.StoreName);
        Assert.Equal(1.70m, basket.Total);
    }

    // The clicked milk costs 1.50 but a similar milk in the same store costs 1.10, so the row shows the cheaper one.
    [Fact]
    public void OwnStoreCheaperSimilarProduct_ReplacesTheClickedOne()
    {
        var milk = I(1, "pienas");
        var cheaper = I(2, "pienas 1l");
        var alpha = S(1, "Alpha");
        var prices = new List<Price> { P(alpha, milk, 1.50m), P(alpha, cheaper, 1.10m) };

        var basket = Service.FindCheapestBasket(new List<Item> { milk, cheaper }, prices, new List<BasketRequestItem> { Row(1, 1, 1) }, Normal);

        var row = Assert.Single(basket.Items);
        Assert.Equal(2, row.ItemId);
        Assert.Equal("pienas 1l", row.ItemName);
        Assert.Equal(1.10m, row.UnitCost);
    }

    // At equal cost the exact name match wins (even with the higher id 9); at equal similarity the lower id wins.
    [Fact]
    public void SameCost_MoreSimilarNameWins_ThenLowerItemId()
    {
        var clicked = I(9, "pienas");
        var similar = I(2, "pienas 1l");
        var alpha = S(1, "Alpha");
        var prices = new List<Price> { P(alpha, similar, 1.00m), P(alpha, clicked, 1.00m) };

        var basket = Service.FindCheapestBasket(new List<Item> { clicked, similar }, prices, new List<BasketRequestItem> { Row(1, 9, 1) }, Normal);

        Assert.Equal(9, Assert.Single(basket.Items).ItemId);

        var twinLater = I(3, "pienas 1l");
        var twinEarlier = I(2, "pienas 1l");
        prices = new List<Price> { P(alpha, clicked, 2.00m), P(alpha, twinLater, 1.00m), P(alpha, twinEarlier, 1.00m) };

        basket = Service.FindCheapestBasket(new List<Item> { clicked, twinLater, twinEarlier }, prices, new List<BasketRequestItem> { Row(1, 9, 1) }, Normal);

        Assert.Equal(2, Assert.Single(basket.Items).ItemId);
    }

    // Both stores total 2.00; Tie-A wins by name even though Tie-B has the lower id and was listed first.
    [Fact]
    public void EqualTotals_StoreWhoseNameSortsFirstWins()
    {
        var kefir = I(1, "kefyras");
        var tieB = S(1, "Tie-B");
        var tieA = S(2, "Tie-A");
        var prices = new List<Price> { P(tieB, kefir, 2.00m), P(tieA, kefir, 2.00m) };

        var basket = Service.FindCheapestBasket(new List<Item> { kefir }, prices, new List<BasketRequestItem> { Row(1, 1, 1) }, Normal);

        Assert.Equal("Tie-A", basket.StoreName);
    }

    // Broad (0.2) accepts the cheaper 0.24 match, normal (0.3) only the 0.33 match, strict (0.5) nothing.
    [Fact]
    public void HigherStrictness_ShrinksCandidates_UntilNoStoreCanFill()
    {
        // against "pienas": "pienas 2.5% 1l" scores 0.33, "pienas 2.5% rieb 1l" scores 0.24
        var clicked = I(1, "pienas");
        var closeMatch = I(2, "pienas 2.5% 1l");
        var farMatch = I(3, "pienas 2.5% rieb 1l");
        var epsilon = S(1, "Epsilon");
        var zeta = S(2, "Zeta");
        var items = new List<Item> { clicked, closeMatch, farMatch };
        var prices = new List<Price> { P(epsilon, closeMatch, 3.00m), P(zeta, farMatch, 2.00m) };
        var rows = new List<BasketRequestItem> { Row(1, 1, 1) };

        Assert.Equal("Zeta", Service.FindCheapestBasket(items, prices, rows, new SimilarityScore(0.2)).StoreName);
        Assert.Equal("Epsilon", Service.FindCheapestBasket(items, prices, rows, new SimilarityScore(0.3)).StoreName);
        Assert.Null(Service.FindCheapestBasket(items, prices, rows, new SimilarityScore(0.5)));
    }

    // Milk 1.20 x2 and bread 0.50 x1 give row totals 2.40 and 0.50 and a basket total of 2.90.
    [Fact]
    public void RowTotalIsUnitCostTimesQuantity_AndTotalIsTheSumOfRows()
    {
        var milk = I(1, "pienas");
        var bread = I(2, "duona");
        var beta = S(1, "Beta");
        var prices = new List<Price> { P(beta, milk, 1.20m), P(beta, bread, 0.50m) };
        var rows = new List<BasketRequestItem> { Row(1, 1, 2), Row(2, 2, 1) };

        var basket = Service.FindCheapestBasket(new List<Item> { milk, bread }, prices, rows, Normal);

        Assert.Equal(new List<decimal> { 2.40m, 0.50m }, basket.Items.Select(i => i.RowTotal));
        Assert.Equal(2.90m, basket.Total);
    }

    // An exaggerated discount exists at store 1 only: asking for store 1 gives Exaggerated, store 2 gives Real.
    [Fact]
    public void VerdictFor_UsesTheStoresOwnFlag_AndRealWhenThereIsNone()
    {
        // retail:10 . 10 x10, 8 x20, 6.
        var store = S(1, "Alpha");
        var fish = I(1, "lasisa");
        var prices = new List<Price> { P(store, fish, 6m) };
        prices[0].RetailCost = 10m;
        prices[0].RecordedAt = Today;
        for (var d = 1; d <= 20; d++)
            prices.Add(P(store, fish, 8m));
        for (var d = 21; d <= 30; d++)
            prices.Add(P(store, fish, 10m));
        for (var d = 1; d <= 30; d++)
            prices[d].RecordedAt = Today.AddDays(-d);

        Assert.Equal(DiscountVerdict.Exaggerated, Service.VerdictFor(prices, 1, Today));
        Assert.Equal(DiscountVerdict.Real, Service.VerdictFor(prices, 2, Today));
    }
}
