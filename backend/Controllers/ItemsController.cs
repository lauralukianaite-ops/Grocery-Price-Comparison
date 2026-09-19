using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemPriceHistoryService _itemPriceHistoryService;
    private readonly ItemSimilarityService _similarityService;
    private readonly AppDbContext _context;
    
    public ItemsController(IItemPriceHistoryService service, ItemSimilarityService similarityService, AppDbContext context)
    {
        _itemPriceHistoryService = service;
        _similarityService = similarityService;
        _context = context;
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

    /// Get similar items based on Tri-gram name similarity algorithm
    [HttpGet("{id}/similar")]
    public async Task<IActionResult> GetSimilarItems(int id, [FromQuery] double threshold = 0.4)
    {
        // Validate threshold
        if (threshold < 0 || threshold > 1)
            return BadRequest("Threshold must be between 0 and 1");

        // Get base item
        var baseItem = await _context.Items.FindAsync(id);
        if (baseItem == null)
            return NotFound($"Item with id {id} not found");

        // Get all items
        var allItems = await _context.Items.ToListAsync();

        // Find similar items
        var similarItems = _similarityService.FindSimilarItems(baseItem, allItems, threshold);

        return Ok(new
        {
            BaseItemId = id,
            BaseItemName = baseItem.Name,
            Threshold = threshold,
            SimilarItems = similarItems,
            Count = similarItems.Count
        });
    }
}