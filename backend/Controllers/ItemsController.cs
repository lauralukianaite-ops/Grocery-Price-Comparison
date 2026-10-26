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

    public ItemsController(IItemPriceHistoryService service, IItemSimilarityService similarityService, IFakeDiscountService fakeDiscountService)
    {
        _itemPriceHistoryService = service;
        _similarityService = similarityService;
        _fakeDiscountService = fakeDiscountService;
    }

    [HttpGet]
    public IActionResult GetItems()
    {
        // TODO: Return the paginated product catalog with its current store prices.
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpGet("{itemId:int}/price-history")]
    public async Task<IActionResult> GetPriceHistory(int itemId)
    {
        var history = await _itemPriceHistoryService.GetPriceHistoryAsync(itemId);
        if (history is null) return NotFound();
        return Ok(history);
    }

    [HttpGet("search/{itemName}")]
    public async Task<IActionResult> GetSimilarItems(string itemName, [FromQuery] double threshold = ItemSimilarityService.DefaultThreshold)
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

    [HttpPost("refresh-index")]
    public async Task<IActionResult> RefreshIndex(
        [FromServices] IItemsRepository repository,
        [FromServices] IItemSimilarityIndex index)
    {
        var items = await repository.GetItemsWithPriceAndStoreAsync();
        index.BuildOrUpdateIndex(items);

        return Ok(new 
        { 
            message = "Index successfully renewed from database.", 
            itemCount = items.Count 
        });
    }
}
