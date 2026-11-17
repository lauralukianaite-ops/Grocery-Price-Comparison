using backend.DTOs;
using backend.Enums;
using backend.Services;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketsController : ControllerBase
{
    private readonly IBasketService _basketService;

    public BasketsController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BasketRequestDto request)
    {
        var problems = await _basketService.FindProblemsAsync(request);
        if (problems.Count > 0) return BadRequest(problems);

        var result = await _basketService.CreateAsync(request);
        if (result.Outcome == BasketOutcome.NoCompleteBasket)
            return UnprocessableEntity("No store has all these items. Try a broader search or a smaller basket.");
        if (result.Outcome == BasketOutcome.NoStoreInRange)
            return UnprocessableEntity($"No store with all these items is within {request.MaxKm ?? BasketService.DefaultMaxKm} km.");
        if (result.Outcome == BasketOutcome.DistanceUnavailable)
            return StatusCode(503, "Travel distance unavailable.");

        return Ok(result.Basket);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var basket = await _basketService.GetAsync(id);
        if (basket is null) return NotFound();
        return Ok(basket);
    }
}
