namespace Infrastructure.Services;

public class TokenService(
    JwtSettings jwtSettings,
    IRepository<UserCredential, Guid> userCredentialRepository,
    IRepository<RefreshToken, Guid> refreshTokenRepository
) : ITokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings;
    private readonly IRepository<UserCredential, Guid> _userCredentialRepository =
        userCredentialRepository;
    private readonly IRepository<RefreshToken, Guid> _refreshTokenRepository =
        refreshTokenRepository;

    public async Task<string?> GenerateAccessTokenAsync(Guid userId)
    {
        UserCredential? userCredential = await _userCredentialRepository.GetByIdAsync(userId);

        if (userCredential == null)
            return null;

        List<Claim> claims =
        [
            new(ClaimTypes.Sid, userCredential.Id.ToString()),
            new(ClaimTypes.Email, userCredential.Email),
            new(ClaimTypes.Name, userCredential.Username),
            new(ClaimTypes.NameIdentifier, userCredential.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        return GenerateToken(
            _jwtSettings.AccessTokenSecret,
            _jwtSettings.AccessTokenExpiration,
            claims
        );
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        string refreshToken = GenerateToken(
            _jwtSettings.RefreshTokenSecret,
            _jwtSettings.RefreshTokenExpiration
        );
        RefreshToken? refresh = await _refreshTokenRepository.GetSingleAsync(x =>
            x.UserId == userId
        );

        if (refresh is null)
        {
            await _refreshTokenRepository.CreateAsync(new RefreshToken(userId, refreshToken));
        }
        else
        {
            //refresh.Token = refreshToken;
            //refresh.DateTime = DateTime.Now;
            //await _refreshTokenRepository.UpdateAsync(refresh);
            await _refreshTokenRepository.UpdateOneAsync(
                t => t.Id == refresh.Id,
                Builders<RefreshToken>
                    .Update.Set(t => t.Token, refreshToken)
                    .Set(t => t.DateTime, DateTime.Now)
            );
        }

        return refreshToken;
    }

    public Task<bool> ValidateAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.AccessTokenSecret);

        try
        {
            tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken
            );

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type.Contains("sid")).Value;
            var userEmail = jwtToken.Claims.First(x => x.Type.Contains("emailaddress")).Value;
            var userName = jwtToken.Claims.First(x => x.Type.Contains("name")).Value;
            var nameId = jwtToken.Claims.First(x => x.Type.Contains("nameidentifier")).Value;

            if (userId == null || userEmail == null || userName == null || nameId == null)
            {
                return Task.FromResult(false);
            }

            // return user id from JWT token if validation successful
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<bool> ValidateRefreshToken(string refreshToken, string token)
    {
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = (JwtSecurityToken)handler.ReadToken(token);

        Guid userId = Guid.Parse(jwtToken.Claims.First(x => x.Type.Contains("sid")).Value);

        RefreshToken? refreshT = await _refreshTokenRepository.GetSingleAsync(x =>
            x.UserId == userId
        );

        if (refreshT == null)
        {
            return false;
        }

        TokenValidationParameters validationParameters =
            new()
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtSettings.RefreshTokenSecret)
                ),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero
            };

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        try
        {
            jwtSecurityTokenHandler.ValidateToken(
                refreshToken,
                validationParameters,
                out SecurityToken tokenValidated
            );
            var jwtRefreshToken = (JwtSecurityToken)tokenValidated;

            if (tokenValidated == null)
            {
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private string GenerateToken(
        string secretKey,
        int expires,
        IEnumerable<Claim> claims = default!
    )
    {
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(secretKey));

        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken securityToken =
            new(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                claims,
                DateTime.Now,
                DateTime.Now.AddMinutes(expires),
                credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}
