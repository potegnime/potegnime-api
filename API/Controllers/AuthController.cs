using API.DTOs.User;
using API.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Fields
        private readonly IAuthService _authService;

        // Constructor
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register"), AllowAnonymous]
        public async Task<ActionResult<string>> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            try
            {
                string token = await _authService.RegisterAsync(userRegisterDto);
                return StatusCode(201, new JwtTokenResponseDto { Token= token });
            }
            catch (ArgumentException e)
            {
                // Missing fields
                return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = e.Message });
            }
            catch (ConflictExceptionDto e)
            {
                // User not found
                return Conflict(new ErrorResponseDto { ErrorCode = 1, Message = e.Message });
            }
            catch (Exception e)
            {
                // Wild card error
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("login"), AllowAnonymous]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginDto userLoginDto)
        {
            try
            {
                var token = await _authService.LoginAsync(userLoginDto);
                return Ok(new JwtTokenResponseDto { Token = token });
            }
            catch (ArgumentException e)
            {
                return StatusCode(401, new ErrorResponseDto { ErrorCode = 1, Message = e.Message });

            }
            catch (Exception e)
            {
                // Wild card error
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("refresh"), Authorize]
        public async Task<ActionResult<string>> RefreshToken()
        {
            try
            {
                var userId = User.FindFirstValue("uid");
                if (userId == null)
                {
                    return Unauthorized("loolll");
                }
                // Generate token
                var token = await _authService.GenerateJwtToken(Convert.ToInt32(userId));
                return Ok(new JwtTokenResponseDto { Token = token });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = "Server error" });
            }
        }
    }
}

