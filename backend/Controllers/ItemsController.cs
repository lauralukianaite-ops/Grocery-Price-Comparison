using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemPriceHistoryService _itemPriceHistoryService;
    
    public ItemsController(IItemPriceHistoryService service)
    {
        _itemPriceHistoryService = service;
    }

    [HttpGet("{itemId}/price-history")]
    public async Task<IActionResult> GetPriceHistory(int itemId)
    {
        var history = await _itemPriceHistoryService.GetPriceHistoryAsync(itemId);
        if (history is null)
        {
            return NotFound();
        }
        return Ok(history);
    }
}