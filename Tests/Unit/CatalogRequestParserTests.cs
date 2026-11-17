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
}