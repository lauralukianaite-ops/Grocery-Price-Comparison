using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly AppDbContext _context;
    
    public UsersRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<AppUser?> GetUserByAuth0IdAsync(string auth0Id)
    {
        return await _context.AppUsers.FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);
    }

    public async Task AddUserAsync(AppUser user)
    {
        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();
    }
}