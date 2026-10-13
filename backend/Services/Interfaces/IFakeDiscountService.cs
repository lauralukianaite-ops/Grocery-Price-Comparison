using backend.DTOs;

namespace backend.Services.Interfaces;

public interface IFakeDiscountService
{
    Task<List<DiscountFlagResponseDto>?> GetDiscountFlag(int itemId);
}