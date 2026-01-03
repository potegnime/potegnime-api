namespace PotegniMe.Services.AuthService;

public interface IAuthService
{
    // Tokens
    // Generate short-lived JWT access token
    string GenerateAccessToken(User user);
    // Generate long-lived refresh token
    Task SetRefreshToken(HttpResponse response, User user);

    // User auth
    Task<string> LoginAsync(UserLoginDto user, HttpResponse response);
    Task<string> RegisterAsync(UserRegisterDto user, HttpResponse response);
    Task LogoutAsync(HttpResponse response, User user);
    // Used to verify login credentials
    Task<bool> VerifyLogin(string username, string password);

    // Forgot & reset password
    Task ForgotPassword(ForgotPasswordDto forgotPasswordDto);
    Task<string> ResetPassword(ResetPasswordDto request);
}
