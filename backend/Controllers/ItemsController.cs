using backend.Repositories;
using backend.Services;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemPriceHistoryService _itemPriceHistoryService;
    private readonly IItemSimilarityService _similarityService;
    private readonly IFakeDiscountService _fakeDiscountService;
    private readonly ICatalogService _catalogService;

    public ItemsController(
        IItemPriceHistoryService service,
        IItemSimilarityService similarityService,
        IFakeDiscountService fakeDiscountService,
        ICatalogService catalogService)
    {
        _itemPriceHistoryService = service;
        _similarityService = similarityService;
        _fakeDiscountService = fakeDiscountService;
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? query = null,
        [FromQuery] string? store = null,
        [FromQuery] string sort = "discount-high",
        [FromQuery] string? ids = null)
    {
        var catalog = await _catalogService.GetCatalogAsync(page, pageSize, query, store, sort, ids);
        return Ok(catalog);
    }

    [HttpGet("{itemId:int}/price-history")]
    public async Task<IActionResult> GetPriceHistory(int itemId)
    {
        var history = await _itemPriceHistoryService.GetPriceHistoryAsync(itemId);
        if (history is null) return NotFound();
        return Ok(history);
    }

    [HttpGet("search/{itemName}")]
    public async Task<IActionResult> GetSimilarItems(
        string itemName,
        [FromQuery] double threshold = ItemSimilarityService.DefaultThreshold)
    {
        var similarItems = await _similarityService.GetSimilarItemsAsync(itemName, threshold);
        return Ok(similarItems);
    }

    [HttpGet("{itemId:int}/discount-check")]
    public async Task<IActionResult> GetDiscountCheck(int itemId)
    {
        var check = await _fakeDiscountService.GetDiscountFlag(itemId);
        if (check is null) return NotFound();
        return Ok(check);
    }

    [HttpGet("stores")]
    public async Task<IActionResult> GetStores()
    {
        var stores = await _catalogService.GetStoresAsync();
        return Ok(stores.Select(s => new {s.Id, s.Name}));
    }

    [HttpPost("refresh-index")]
    public async Task<IActionResult> RefreshIndex()
    {
        await _similarityService.RefreshIndexAsync();
        return NoContent();
    }
}
