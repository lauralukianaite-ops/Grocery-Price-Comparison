using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class StoresRepository : IStoresRepository
{
    private readonly AppDbContext _context;

    public StoresRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Store>> GetAllAsync() =>
        _context.Stores.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
}