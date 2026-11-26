using System.Security.Claims;
using backend.Entities;
using backend.Repositories;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService : IUserService
{
    private readonly IUsersRepository _usersRepository;
    private const string EmailClaim = "https://grocery-price-comparison-api/email";
    
    public UserService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }
    
    public async Task<AppUser> GetOrCreateCurrentUserAsync(ClaimsPrincipal principal)
    {
        var auth0Id = principal.FindFirst("sub")?.Value ?? throw new InvalidOperationException("No sub claim found");
        var email = principal.FindFirst(EmailClaim)?.Value ?? throw new InvalidOperationException("No email claim found");
        
        var user = await _usersRepository.GetUserByAuth0IdAsync(auth0Id);
        if (user == null)
        {
            user = new AppUser
            {
                Auth0Id = auth0Id,
                Email = email
            };
            try
            {
                await _usersRepository.AddUserAsync(user);
            }
            catch (DbUpdateException)
            {
                user = await _usersRepository.GetUserByAuth0IdAsync(auth0Id);
                if (user == null) throw;
            }
            
        }
        return user;
    }
}