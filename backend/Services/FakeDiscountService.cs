using backend.Data;
using backend.DTOs;

namespace backend.Services;

public class FakeDiscountService : IFakeDiscountService
{
    private readonly AppDbContext _dbContext;
    public FakeDiscountService(AppDbContext db)
    {
        _dbContext = db;
    }

    public async Task<List<DiscountFlagResponseDto>?> GetDiscountFlag(int itemId)
    {
        throw new NotImplementedException();
    }
}