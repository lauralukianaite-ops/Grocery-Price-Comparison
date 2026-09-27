using backend.DTOs;

namespace backend.Services;

public interface IFakeDiscountService
{
    Task<List<DiscountFlagResponseDto>?> GetDiscountFlag(int itemId);
}