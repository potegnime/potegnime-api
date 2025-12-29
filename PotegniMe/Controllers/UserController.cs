using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PotegniMe.DTOs.User;
using PotegniMe.Services.AuthService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController(IUserService userService, IAuthService authService) : ControllerBase
    {
        [HttpGet("username"), Authorize]
        public async Task<ActionResult> GetUser(string username)
        {
            try
            {
                User user = await userService.GetUserByUsername(username);
                return Ok(new GetUser
                {
                    Username = user.Username,
                    Joined = user.JoinedDate.Date.ToShortDateString(),
                    Role = Convert.ToString(user.Role.Name),
                    HasPfp = user.ProfilePicFilePath != null
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

        [HttpPost("updateUser"), Authorize]
        public async Task<ActionResult<string>> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var username = User.FindFirstValue("username");
            var email = User.FindFirstValue("email");
            if (string.IsNullOrWhiteSpace(username)) return Unauthorized();
            
            // check if there is data to update
            if (string.IsNullOrWhiteSpace(updateUserDto.Username) && string.IsNullOrWhiteSpace(updateUserDto.Email))
            {
                return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Ni podatkov za posodobitev." });
            }
            
            var newUsername = updateUserDto.Username?.Trim();
            var newEmail = updateUserDto.Email?.Trim();
            
            if (!string.IsNullOrEmpty(newUsername) && string.Equals(username, newUsername, StringComparison.Ordinal))
            {
                return Conflict(new ErrorResponseDto { ErrorCode = 1, Message = "Novo uporabniško ime ne sme biti enako prejšnjemu!" });
            }

            if (!string.IsNullOrEmpty(newEmail) &&
                string.Equals(email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new ErrorResponseDto { ErrorCode = 1, Message = "Nov e-poštni naslov ne sme biti enak prejšnjemu!" });
            }
            
            try
            {
                if (!string.IsNullOrEmpty(newEmail)) await userService.UpdateEmail(username, newEmail);
                if (!string.IsNullOrEmpty(newUsername))
                {
                    await userService.UpdateUsername(username, newUsername);
                    // update pfp as well - pfp name is username (https://api.potegni.me/pfp/username)
                    await userService.RenamePfp(username, newUsername);
                }
                
                // return new JWT (with new username/email)
                string token = await authService.GenerateJwtToken(newUsername ?? username);
                return Ok(new JwtTokenResponseDto { Token = token });
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
        
        [HttpPost("setPfp"), Authorize]
        public async Task<ActionResult<string>> UpdatePfp([FromForm] UpdatePfpDto updatePfpDto)
        {
            try
            {
                // Check if user exists
                var username = User.FindFirstValue("username");
                if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

                // Check if profile picture is attached - determine between update and remove profile picture
                if (updatePfpDto.ProfilePicFile == null)
                {
                    // Remove profile picture
                    await userService.RemovePfp(username);
                    return Ok(new JwtTokenResponseDto
                    {
                        Token = await authService.GenerateJwtToken(username)
                    });
                }
                // Update profile picture
                await userService.UpdatePfp(username, updatePfpDto.ProfilePicFile);
                
                return Ok(new JwtTokenResponseDto
                {
                    Token = await authService.GenerateJwtToken(username)
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpPost("updatePassword"), Authorize]
        public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto)
        {
            try
            {
                // Check if user exists
                var username = User.FindFirstValue("username");
                if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

                // Old password cannot be the same as new password
                if (updatePasswordDto.NewPassword == updatePasswordDto.OldPassword)
                {
                    return StatusCode(403, new ErrorResponseDto { ErrorCode = 1, Message = "Novo geslo ne sme biti enako prejšnjemu" });
                }

                // Verify the old password is correct
                if (!await authService.VerifyLogin(username, updatePasswordDto.OldPassword))
                {
                    return StatusCode(403, new ErrorResponseDto { ErrorCode = 1, Message = "Staro geslo ni pravilno!" });

                }

                await userService.UpdatePassword(username, updatePasswordDto.NewPassword);
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
        public async Task<ActionResult> DeleteUser([FromBody] DeleteUserDto deleteUserDto)
        {
            try
            {
                // Check if user exists
                var username = User.FindFirstValue("username");
                if (username == null) return Unauthorized();

                // Verify password
                if (!await authService.VerifyLogin(username, deleteUserDto.Password))
                {
                    return StatusCode(403, new ErrorResponseDto { ErrorCode = 1, Message = "Geslo ni pravilno" });
                }

                await userService.DeleteUser(username);
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

        [HttpPost("submitUploaderRequest"), Authorize]
        public async Task<ActionResult> SubmitUploaderRequest([FromBody] UploaderRequestDto uploaderRequestDto)
        {
            var username = User.FindFirstValue("username");
            if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

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
                // Check if user is already uploader or admin
                if (await userService.IsUploader(username) || await userService.IsAdmin(username))
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Uporabnik je že uploader ali admin!" });
                }

                // Store new uploader request to db
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }
    }
}
