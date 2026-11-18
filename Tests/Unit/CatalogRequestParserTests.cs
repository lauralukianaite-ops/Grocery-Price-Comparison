using System.Collections.Generic;
using backend.Enums;
using backend.Services;
using Xunit;

namespace backend.Tests;

public class CatalogRequestParserTests
{
    [Theory]
    [InlineData("price-low", CatalogSort.PriceLow)]
    [InlineData("price-high", CatalogSort.PriceHigh)]
    [InlineData("name", CatalogSort.Name)]
    [InlineData("name-desc", CatalogSort.NameDescending)]
    [InlineData("discount-low", CatalogSort.DiscountLow)]
    [InlineData("discount-high", CatalogSort.DiscountHigh)]
    [InlineData("nonsense", CatalogSort.DiscountHigh)]
    [InlineData(null, CatalogSort.DiscountHigh)]
    public void ParseSort_MapsKnownValuesAndDefaultsToDiscountHigh(string? input, CatalogSort expected)
    {
        Assert.Equal(expected, CatalogRequestParser.ParseSort(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseItemIds_ReturnsNullForEmptyInput(string? input)
    {
        Assert.Null(CatalogRequestParser.ParseItemIds(input));
    }

    [Fact]
    public void ParseItemIds_SkipsInvalidValuesAndRemovesDuplicates()
    {
        var result = CatalogRequestParser.ParseItemIds("1, 2,abc,2,,3");

        Assert.Equal(new List<int> { 1, 2, 3 }, result);
    }

    [Fact]
    public void TryParseStoreIds_RemovesDuplicatesAndTrimsValues()
    {
        Assert.True(CatalogRequestParser.TryParseStoreIds("1, 2,1", out var storeIds));
        Assert.Equal(new[] { 1, 2 }, storeIds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParseStoreIds_AcceptsAnEmptySelection(string? input)
    {
        Assert.True(CatalogRequestParser.TryParseStoreIds(input, out var storeIds));
        Assert.Null(storeIds);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("invalid")]
    [InlineData("1,")]
    public void TryParseStoreIds_RejectsInvalidValues(string input)
    {
        Assert.False(CatalogRequestParser.TryParseStoreIds(input, out _));
    }
}
