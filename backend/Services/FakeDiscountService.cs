using backend.Data;

namespace backend.Services;

public class FakeDiscountService : IFakeDiscountService
{
    private readonly AppDbContext _dbContext;
    public FakeDiscountService(AppDbContext db)
    {
        _dbContext = db;
    }
}