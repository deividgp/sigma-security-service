namespace API.Controllers;

[ApiController]
public class AuthenticationController(
    IAuthenticationService authenticationService,
    ITokenService tokenService
) : ControllerBase
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost("/api/Auth/SignUp")]
    public async Task<ActionResult> SignUp(UserCredentialSignupDTO userCredential)
    {
        return Ok(await _authenticationService.SignUp(userCredential));
    }

    [HttpPost("/api/Auth/LogIn")]
    public async Task<ActionResult> LogIn(UserCredentialLoginDTO userCredential)
    {
        var result = await _authenticationService.LogIn(userCredential);

        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("/api/Auth/RefreshToken")]
    public async Task<ActionResult> RefreshToken(string refreshToken)
    {
        if (refreshToken == null)
            return BadRequest();

        if (string.IsNullOrEmpty(Request.Headers.Authorization))
            return Forbid();

        return Ok(
            await _authenticationService.RefreshToken(
                Request.Headers.Authorization.ToString().Split(' ')[1],
                refreshToken
            )
        );
    }

    [HttpPost("/api/Auth/ValidateToken")]
    public async Task<ActionResult> ValidateToken()
    {
        if (string.IsNullOrEmpty(Request.Headers.Authorization))
            return Forbid();

        return Ok(
            await _tokenService.ValidateAccessToken(
                Request.Headers.Authorization.ToString().Split(' ')[1]
            )
        );
    }
}
