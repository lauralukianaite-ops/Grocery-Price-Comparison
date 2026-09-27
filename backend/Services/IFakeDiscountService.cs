using backend.DTOs;
using backend.Entities;

namespace backend.Services;

public interface IFakeDiscountService
{
    Task<List<DiscountFlagResponseDto>?> GetDiscountFlag(int itemId);
    List<DiscountFlagResponseDto> FindDiscountFlags(List<Price> prices, DateTime today);
}