namespace backend.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string Auth0Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}