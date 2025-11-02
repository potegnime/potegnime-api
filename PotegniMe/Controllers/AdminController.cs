using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PotegniMe.DTOs.User;
using PotegniMe.Services.AdminService;
using PotegniMe.Services.AuthService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers
{
    [Route("admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        // Fields
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;

        // Constructor
        public AdminController(IAuthService authService, IUserService userService, IAdminService adminService)
        {
            _authService = authService;
            _userService = userService;
            _adminService = adminService;
        }

        [HttpPost("updateRole"), Authorize]
        public async Task<ActionResult> UpdateRole([FromBody] UpdateRoleDto updateRoleDto)
        {
            // Check if user is admin
            var uid = User.FindFirstValue("uid");
            if (uid == null) return Unauthorized();

            if (!await _userService.IsAdmin(Convert.ToInt32(uid)))
            {
                return Unauthorized();
            }

            try
            {
                updateRoleDto.RoleName = updateRoleDto.RoleName.ToLower();
                if (
                    updateRoleDto.RoleName != "admin" &&
                    updateRoleDto.RoleName != "user" &&
                    updateRoleDto.RoleName != "uploader"
                    )
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "RoleName mora vsebovati vrednost admin ali uporabnik" });
                }

                Claim claim = new Claim("uid", Convert.ToString(updateRoleDto.UserId));
                await _adminService.UpdateRole(claim, updateRoleDto.RoleName);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpDelete("adminDelete"), Authorize]
        public async Task<ActionResult> AdminDelete(string username)
        {
            // Check if user is admin
            var uid = User.FindFirstValue("uid");
            if (uid == null) return Unauthorized();

            if (!await _userService.IsAdmin(Convert.ToInt32(uid)))
            {
                return Unauthorized();
            }

            try
            {
                User userToDelete = await _userService.GetUserByUsername(username);
                if (userToDelete != null)
                {
                    await _userService.DeleteUser(Convert.ToInt32(userToDelete.UserId));
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("uploaderRequest"), Authorize]
        public async Task<ActionResult<string>> UploaderRequests([FromBody] UserRegisterDto userRegisterDto)
        {
            try
            {
                string token = await _authService.RegisterAsync(userRegisterDto);
                return StatusCode(201, new JwtTokenResponseDto { Token = token });
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
    }
}

