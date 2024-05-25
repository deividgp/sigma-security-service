namespace Domain.Entities;

public class RefreshToken(Guid userId, string token) : Entity<Guid>
{
    public Guid UserId { get; set; } = userId;

    public string Token { get; set; } = token;

    public DateTime DateTime { get; set; } = DateTime.Now;
}
