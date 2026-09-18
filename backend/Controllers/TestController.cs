using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _context;

    public TestController(AppDbContext context)
    {
        _context = context;
    }

    //Connection Check
    [HttpGet("db-check")]
    public async Task<IActionResult> CheckDatabase()
    {
        var result = await _context.Database.SqlQueryRaw<string>("SELECT version()").ToListAsync();
        return Ok(new { DbVersion = result.FirstOrDefault() });
    }

    //Variable Extraction (Item Name and Price)
    [HttpGet("read-price")]
    public async Task<IActionResult> GetFirstPrice()
    {
        // Trying to get the first price from the database
        var priceEntity = await _context.Prices
            .Include(p => p.Item)
            .FirstOrDefaultAsync();

        string itemName;
        decimal itemPrice;

        // If the database is empty, assign test values
        if (priceEntity == null)
        {
            itemName = "Testinis Pienas 2.5%";
            itemPrice = 1.49m;
        }
        else
        {
            // If data is available, extract from DB
            itemName = priceEntity.Item?.Name ?? "Name not found";
            itemPrice = priceEntity.Amount;
        }

        // Return the extracted variables
        return Ok(new 
        { 
            Message = priceEntity == null ? "DB is still empty, returning fake values" : "Data extracted from DB!",
            ExtractedName = itemName, 
            ExtractedPrice = itemPrice
        });
    }
}