﻿using API.DTOs.User;
using API.Services.AdminService;
using API.Services.AuthService;
using API.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers.Admin
{
    [Route("api/admin")]
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
        public async Task<ActionResult> UpdateRole([FromForm] UpdateRoleDto updateRoleDto)
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

        [HttpPost("uploaderRequests"), AllowAnonymous]
        public async Task<ActionResult<string>> UploaderRequests([FromBody] UserRegisterDto userRegisterDto)
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
    }
}

