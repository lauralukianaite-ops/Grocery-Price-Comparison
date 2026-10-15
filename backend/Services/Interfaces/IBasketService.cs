using backend.DTOs;

namespace backend.Services.Interfaces;

public interface IBasketService
{
    Task<List<string>> FindProblemsAsync(BasketRequestDto request);
    Task<BasketResponseDto?> CreateAsync(BasketRequestDto request);
    Task<BasketResponseDto?> GetAsync(Guid id);
}
