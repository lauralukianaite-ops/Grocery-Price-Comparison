using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class BasketsRepository : IBasketsRepository
{
    private readonly AppDbContext _context;

    public BasketsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(Basket basket)
    {
        basket.Id = Guid.NewGuid();
        _context.Baskets.Add(basket);
        await _context.SaveChangesAsync();
    }

    // Rows are sorted by their stored position, so a link always shows them in the order they were generated.
    public async Task<Basket?> GetByIdAsync(Guid id)
    {
        return await _context.Baskets
            .Include(b => b.Items.OrderBy(i => i.Position))
            .Include(b => b.RequestItems.OrderBy(i => i.Position))
            .Include(b => b.BannedItems)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
