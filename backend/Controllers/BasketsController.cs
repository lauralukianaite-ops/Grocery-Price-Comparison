using backend.DTOs;
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

        var basket = await _basketService.CreateAsync(request);
        if (basket is null) return UnprocessableEntity("No store has all these items. Try a broader search or a smaller basket.");

        return Ok(basket);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var basket = await _basketService.GetAsync(id);
        if (basket is null) return NotFound();
        return Ok(basket);
    }
}
