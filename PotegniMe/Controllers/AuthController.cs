using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PotegniMe.Core.Exceptions;
using PotegniMe.Services.AuthService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers;

[Route("auth")]
[ApiController]
public class AuthController(IAuthService authService, IUserService userService) : ControllerBase
{
    [HttpPost("register"), AllowAnonymous]
    public async Task<ActionResult<string>> Register([FromBody] UserRegisterDto userRegisterDto)
    {
        string token = await authService.RegisterAsync(userRegisterDto, Response);
        return StatusCode(201, new JwtTokenResponseDto { AccessToken= token });
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<ActionResult<string>> Login([FromBody] UserLoginDto userLoginDto)
    {
        var token = await authService.LoginAsync(userLoginDto, Response);
        return Ok(new JwtTokenResponseDto { AccessToken = token });
    }

    [HttpPost("refresh"), AllowAnonymous]
    public async Task<ActionResult<JwtTokenResponseDto>> RefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            // Cookie expired on client or not present
            return Unauthorized();
        }
        User user = await userService.GetUserByRefreshToken(refreshToken);
        if (user.RefreshTokenExpiration < DateTime.UtcNow) return Unauthorized();
        
        var accessToken = authService.GenerateAccessToken(user);
        await authService.SetRefreshToken(Response, user);
        return Ok(new JwtTokenResponseDto { AccessToken = accessToken });
    }

    [HttpPost("forgotPassword"), AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        await authService.ForgotPassword(forgotPasswordDto);
        return Ok();
    }

    [HttpPost("logout"), Authorize]
    public async Task<ActionResult> Logout()
    {
        User user = await GetCurrentUserAsync();
        await authService.LogoutAsync(Response, user);
        return Ok();
    }

    [HttpPost("resetPassword"), AllowAnonymous]
    public async Task<ActionResult<string>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        string accessToken = await authService.ResetPassword(resetPasswordDto);
        return StatusCode(201, new JwtTokenResponseDto { AccessToken = accessToken });
    }
    
    // Helpers
    private async Task<User> GetCurrentUserAsync()
    {
        var uid = User.FindFirstValue("uid");
        if (uid == null) throw new UnauthorizedException("Unauthorized");

        return await userService.GetUserById(Convert.ToInt32(uid));
    }
}
