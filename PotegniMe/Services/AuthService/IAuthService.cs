namespace PotegniMe.Services.AuthService;

public interface IAuthService
{
    // Generate short-lived JWT access token
    string GenerateAccessToken(User user);
    
    // Generate long-lived refresh token
    Task SetRefreshToken(HttpResponse response, User user);

    // Login user
    Task<string> LoginAsync(UserLoginDto user, HttpResponse response);

    // Register user
    Task<string> RegisterAsync(UserRegisterDto user, HttpResponse response);
    
    Task LogoutAsync(HttpResponse response, User user);
    
    // Verify login - used to verify login credentials
    Task<bool> VerifyLogin(string username, string password);

    // Forgot password
    Task ForgotPassword(ForgotPasswordDto forgotPasswordDto);

    // Reset password
    Task<string> ResetPassword(ResetPasswordDto request);
}
