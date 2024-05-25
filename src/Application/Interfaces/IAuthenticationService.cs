namespace Application.Interfaces;

public interface IAuthenticationService
{
    public Task<TokenData> SignUp(UserCredentialSignupDTO userCredential);
    public Task<TokenData?> LogIn(UserCredentialLoginDTO userCredential);
    public Task<TokenData> RefreshToken(string token, string refreshToken);
}
