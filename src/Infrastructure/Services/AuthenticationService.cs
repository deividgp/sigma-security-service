using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Infrastructure.Services;

public class AuthenticationService(
    ITokenService tokenService,
    IRepository<UserCredential, Guid> userCredentialRepository,
    IMapper mapper,
    IConfiguration config
) : IAuthenticationService
{
    private readonly ITokenService _tokenService = tokenService;
    private readonly IRepository<UserCredential, Guid> _userCredentialRepository =
        userCredentialRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IConfiguration _config = config;

    public async Task<TokenData?> LogIn(UserCredentialLoginDTO userCredential)
    {
        List<UserCredential> credentialList =
        [
            .. (
                await _userCredentialRepository.GetAllAsync(u =>
                    (
                        u.Username == userCredential.UsernameEmail
                        || u.Email == userCredential.UsernameEmail
                    )
                )
            )
        ];

        UserCredential? auxCredential = credentialList
            .Where(c =>
                BCrypt.Net.BCrypt.Verify(
                    userCredential.Password,
                    c.Password,
                    false,
                    BCrypt.Net.HashType.SHA384
                )
            )
            .SingleOrDefault();

        return auxCredential is null ? null : await Authenticate(auxCredential.Id);
    }

    public async Task<TokenData> RefreshToken(string token, string refreshToken)
    {
        if (!await _tokenService.ValidateRefreshToken(refreshToken, token))
        {
            throw new Exception();
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var sidClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Contains("sid"))!.Value;

        return await Authenticate(Guid.Parse(sidClaim));
    }

    public async Task<TokenData> SignUp(UserCredentialSignupDTO userCredential)
    {
        UserCredential auxCredential = _mapper.Map<UserCredential>(userCredential);
        auxCredential.Id = Guid.NewGuid();
        auxCredential.Password = BCrypt.Net.BCrypt.HashPassword(auxCredential.Password);

        await _userCredentialRepository.CreateAsync(auxCredential);

        TokenData tokenData = await Authenticate(auxCredential.Id);

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenData.AccessToken
        );
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        string url = _config.GetValue<string>("User:Url") + "Create";
        var response = await httpClient.PostAsJsonAsync(url, "");
        response.EnsureSuccessStatusCode();

        return tokenData;
    }

    private async Task<TokenData> Authenticate(Guid userId)
    {
        return new TokenData
        {
            AccessToken = await _tokenService.GenerateAccessTokenAsync(userId),
            RefreshToken = await _tokenService.GenerateRefreshTokenAsync(userId),
        };
    }
}
