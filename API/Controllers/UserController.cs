using API.DTOs.User;
using API.Services.UserService;
using API.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // Fields
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        // Constructor
        public UserController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        // Routes
        [HttpGet("userId"), AllowAnonymous]
        public async Task<ActionResult> GetUser(int userId)
        {
            try
            {
                User user = await _userService.GetUserById(userId);
                return Ok(new GetUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Joined = user.JoinedDate.Date.ToShortDateString(),
                    Role = Convert.ToString(user.Role.Name)
                });
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("username"), AllowAnonymous]
        public async Task<ActionResult> GetUser(string username)
        {
            try
            {
                User user = await _userService.GetUserByUsername(username);
                return Ok(new GetUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Joined = user.JoinedDate.Date.ToShortDateString(),
                    Role = Convert.ToString(user.Role.Name)
                });
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("pfp/{userId}"), Authorize]
        public async Task<IActionResult> GetUserPfp(int userId)
        {
            try
            {
                var (stream, mimeType) = await _userService.GetPfpStreamWithMime(userId);
                if (stream == null || mimeType == null)
                {
                    return NotFound();
                }
                return File(stream, mimeType);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("updateUsername"), Authorize]
        public async Task<ActionResult> UpdateUsername([FromForm] UpdateUsernameDto updateUsernameDto)
        {
            try
            {
                // Check if user exists
                var userId = User.FindFirstValue("uid");
                if (userId == null)
                {
                    return Unauthorized();
                }

                // Check if current username is the same
                var oldUsername = User.FindFirstValue("username");
                if (oldUsername == updateUsernameDto.Username)
                {
                    return Conflict(new ErrorResponseDto { 
                        ErrorCode = 1, 
                        Message = "Novo uporabniško ime ne sme biti enako prejšnjemu!" 
                    });
                }
                
                var claim = new Claim("uid", userId);
                await _userService.UpdateUsername(claim, updateUsernameDto.Username);
                return Ok();
            }
            catch (ConflictExceptionDto e)
            {
                return Conflict(new ErrorResponseDto { ErrorCode = 1, Message = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("updateEmail"), Authorize]
        public async Task<ActionResult> UpdateEmail([FromForm] UpdateEmailDto updateEmailDto)
        {
            try
            {
                // Check if user exists
                var userId = User.FindFirstValue("uid");
                if (userId == null)
                {
                    return Unauthorized();
                }

                // Check if current email is the same
                var oldEmail = User.FindFirstValue("email");
                if (oldEmail == updateEmailDto.Email)
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Nov e-poštni naslov ne sme biti enak prejšnjemu!" });
                }

                var claim = new Claim("uid", userId);
                await _userService.UpdateEmail(claim, updateEmailDto.Email);
                return Ok();
            }
            catch (ConflictExceptionDto e)
            {
                return Conflict(new ErrorResponseDto { ErrorCode = 1, Message = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("updatePfp"), Authorize]
        public async Task<ActionResult> UpdatePfp([FromForm] UpdatePfpDto updatePfpDto)
        {
            try
            {
                // Check if user exists
                var userId = User.FindFirstValue("uid");
                if (userId == null)
                {
                    return Unauthorized();
                }

                var claim = new Claim("uid", userId);
                // Check if profile picture is attached - determine between update and remove profile picture
                if (updatePfpDto.ProfilePicFile == null)
                {
                    // Remove profile picture
                    await _userService.RemovePfp(claim);
                    return Ok();
                }
                else
                {
                    // Update profile picture
                    await _userService.UpdatePfp(claim, updatePfpDto.ProfilePicFile);
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("updatePassword"), Authorize]
        public async Task<ActionResult> UpdatePassword([FromForm] UpdatePasswordDto updatePasswordDto)
        {
            try
            {
                // Check if user exists
                var userId = User.FindFirstValue("uid");
                if (userId == null)
                {
                    return Unauthorized();
                }

                // Old password cannot be the same as new password
                if (updatePasswordDto.NewPassword == updatePasswordDto.OldPassword)
                {
                    return StatusCode(403, new ErrorResponseDto { ErrorCode = 1, Message = "Novo geslo ne sme biti enako prejšnjemu" });
                }

                // Verify the old password is correct
                var username = User.FindFirstValue("username") ?? throw new ArgumentNullException();
                if (! await _authService.VerifyLogin(username, updatePasswordDto.OldPassword))
                {
                    return StatusCode(403, new ErrorResponseDto { ErrorCode = 1, Message = "Staro geslo ni pravilno!" });

                }

                var claim = new Claim("uid", userId);
                await _userService.UpdatePassword(claim, updatePasswordDto.NewPassword);
                return Ok();
            }
            catch (Exception e)
            {

                if (e is ArgumentNullException)
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Uporabniško ime je prazno!" });
                }
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpDelete("deleteUser"), Authorize]
        public async Task<ActionResult> DeleteUser([FromForm] DeleteUserDto deleteUserDto)
        {
            try
            {
                // Check if user exists
                var userId = User.FindFirstValue("uid");
                if (userId == null)
                {
                    return Unauthorized();
                }

                // Verify password
                var username = User.FindFirstValue("username") ?? throw new ArgumentNullException();
                if (!await _authService.VerifyLogin(username, deleteUserDto.Password))
                {
                    return StatusCode(403, new ErrorResponseDto { ErrorCode = 1, Message = "Geslo ni pravilno" });
                }

                var claim = new Claim("uid", userId);
                await _userService.DeleteUser(Convert.ToInt32(userId));
                return Ok();
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException)
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Uporabniško ime je prazno!" });
                }
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
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message= "RoleName mora vsebovati vrednost admin ali uporabnik" });
                }

                Claim claim = new Claim("uid", Convert.ToString(updateRoleDto.UserId));
                await _userService.UpdateRole(claim, updateRoleDto.RoleName);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("submitUploaderRequest"), Authorize]
        public async Task<ActionResult> SubmitUploaderRequest([FromForm] UploaderRequestDto uploaderRequestDto)
        {
            var uid = User.FindFirstValue("uid");
            if (uid == null) return Unauthorized();

            // Check input length
            if (
                uploaderRequestDto.Experience.Length > 1000 ||
                uploaderRequestDto.Content.Length > 1000 || 
                uploaderRequestDto.Proof?.Length > 3000 || 
                uploaderRequestDto.OtherTrackers?.Length > 1000
                )
            {
                return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Maksimalna dolžina presežena!" });
            }

            try
            {
                // TODO
                // Check if user is already an uploader
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }
    }
}
