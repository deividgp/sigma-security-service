namespace Infrastructure;

public class JwtSettings
{
    public string AccessTokenSecret { get; set; } = default!;
    public string RefreshTokenSecret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int AccessTokenExpiration { get; set; }
    public int RefreshTokenExpiration { get; set; }
}
