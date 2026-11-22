using backend.Entities;

namespace backend.Repositories;

public interface IUsersRepository
{
    Task<AppUser?> GetUserByAuth0IdAsync(string auth0Id);
    Task AddUserAsync(AppUser user);
}