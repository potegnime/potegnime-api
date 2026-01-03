using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PotegniMe.Core.Exceptions;
using PotegniMe.DTOs.User;
using PotegniMe.Services.AdminService;
using PotegniMe.Services.AuthService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers;

[Route("admin")]
[ApiController]
public class AdminController(IAuthService authService, IUserService userService, IAdminService adminService) : ControllerBase
{
    [HttpPost("updateRole"), Authorize]
    public async Task<ActionResult> UpdateRole([FromBody] UpdateRoleDto updateRoleDto)
    {
        // Check if user is admin
        User user = await GetCurrentUserAsync();
        if (!await userService.IsAdmin(user.Username)) return Unauthorized();
        
        updateRoleDto.RoleName = updateRoleDto.RoleName.ToLower();
        if (updateRoleDto.RoleName != "admin" && updateRoleDto.RoleName != "user" && updateRoleDto.RoleName != "uploader")
        {
            return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "RoleName mora vsebovati vrednost admin ali uporabnik" });
        }

        await adminService.UpdateRole(updateRoleDto.Username, updateRoleDto.RoleName);
        return Ok();
    }

    [HttpDelete("adminDelete"), Authorize]
    public async Task<ActionResult> AdminDelete(string username)
    {
        // Check if user is admin
        User user = await GetCurrentUserAsync();
        if (!await userService.IsAdmin(user.Username)) return Unauthorized();
        
        await userService.GetUserByUsername(username); // throws NotFoundException if user doesn't exist
        await userService.DeleteUser(username);
        return Ok();
    }

    [HttpPost("uploaderRequest"), Authorize]
    public async Task<ActionResult<string>> UploaderRequests([FromBody] UserRegisterDto userRegisterDto)
    {
        string token = await authService.RegisterAsync(userRegisterDto, Response);
        return StatusCode(201, new JwtTokenResponseDto { AccessToken = token });
    }
    
    // Helpers
    private async Task<User> GetCurrentUserAsync()
    {
        var uid = User.FindFirstValue("uid");
        if (uid == null) throw new UnauthorizedException("Unauthorized");

        return await userService.GetUserById(Convert.ToInt32(uid));
    }
}
