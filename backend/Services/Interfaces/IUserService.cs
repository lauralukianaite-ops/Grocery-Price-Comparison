using System.Security.Claims;
using backend.Entities;

namespace backend.Services.Interfaces;

public interface IUserService
{
    Task<AppUser> GetOrCreateCurrentUserAsync(ClaimsPrincipal principal);
}