using backend.DTOs;

namespace backend.Services;

public interface IFakeDiscountService
{
    Task<DiscountFlagResponseDto> GetDiscountFlag(int itemId);
}