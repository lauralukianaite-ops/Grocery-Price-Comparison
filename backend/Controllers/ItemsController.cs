using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemPriceHistoryService _itemPriceHistoryService;
    private readonly IItemSimilarityService _similarityService;
    private readonly IFakeDiscountService _fakeDiscountService;

    public ItemsController(IItemPriceHistoryService service, IItemSimilarityService similarityService, IFakeDiscountService fakeDiscountService)
    {
        _itemPriceHistoryService = service;
        _similarityService = similarityService;
        _fakeDiscountService = fakeDiscountService;
    }

    [HttpGet("{itemId:int}/price-history")]
    public async Task<IActionResult> GetPriceHistory(int itemId)
    {
        var history = await _itemPriceHistoryService.GetPriceHistoryAsync(itemId);
        if (history is null)
            return NotFound();

        return Ok(history);
    }

    [HttpGet("search/{itemName}")]
    public async Task<IActionResult> GetSimilarItems(string itemName, [FromQuery] double threshold = 0.3)
    {
        var similarItems = await _similarityService.GetSimilarItemsAsync(itemName, threshold);
        return Ok(similarItems);
    }

    [HttpGet("{itemId:int}/discount-check")]
    public async Task<IActionResult> GetDiscountCheck(int itemId)
    {
        var check = await _fakeDiscountService.GetDiscountFlag(itemId);
        return Ok(check);
    }
}