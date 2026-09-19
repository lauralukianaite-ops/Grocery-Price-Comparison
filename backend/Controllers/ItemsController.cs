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

    /// Get similar items based on Tri-gram name similarity algorithm
    [HttpGet("{id}/similar")]
    public async Task<IActionResult> GetSimilarItems(int id, [FromQuery] double threshold = 0.4)
    {
        // Validate threshold
        if (threshold < 0 || threshold > 1)
            return BadRequest("Threshold must be between 0 and 1");

        var similarItems = await _similarityService.GetSimilarItemsAsync(id, threshold);

        if (similarItems is null)
            return NotFound($"Item with id {id} not found");

        return Ok(similarItems);
    }
}