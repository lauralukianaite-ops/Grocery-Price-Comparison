using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemPriceHistoryService _itemPriceHistoryService;
    private readonly IItemSimilarityService _similarityService;

    public ItemsController(IItemPriceHistoryService service, IItemSimilarityService similarityService)
    {
        _itemPriceHistoryService = service;
        _similarityService = similarityService;
    }

    [HttpGet("{itemId}/price-history")]
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
}