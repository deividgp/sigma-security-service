namespace Domain.Entities;

public class UserCredential : Entity<Guid>
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
