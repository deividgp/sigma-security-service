namespace Application.Interfaces;

public interface ITokenService
{
    public Task<string?> GenerateAccessTokenAsync(Guid userId);
    public Task<string> GenerateRefreshTokenAsync(Guid userId);
    public Task<bool> ValidateAccessToken(string token);
    public Task<bool> ValidateRefreshToken(string refreshToken, string token);
}
