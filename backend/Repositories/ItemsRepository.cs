using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class ItemsRepository : IItemsRepository
{
    private readonly AppDbContext _context;

    public ItemsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Item>> GetItemsWithStoreAsync()
    {
        return await _context.Items
            .Include(i => i.Prices)
            .ThenInclude(p => p.Store)
            .ToListAsync();
    }
}