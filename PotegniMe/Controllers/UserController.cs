using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PotegniMe.Core.Exceptions;
using PotegniMe.DTOs.User;
using PotegniMe.Services.AuthService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers;

[Route("user")]
[ApiController]
public class UserController(IUserService userService, IAuthService authService) : ControllerBase
{
    [HttpGet("username"), Authorize]
    public async Task<ActionResult> GetUser(string username)
    {
        var user = await userService.GetUserByUsername(username);

        return Ok(new GetUser
        {
            Username = user.Username,
            Joined = user.JoinedDate.Date.ToShortDateString(),
            Role = user.Role.Name,
            HasPfp = user.ProfilePicFilePath != null
        });
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
            throw new ArgumentException("Ni podatkov za posodobitev.");
        }
        
        var newUsername = updateUserDto.Username?.Trim();
        var newEmail = updateUserDto.Email?.Trim();
        
        if (!string.IsNullOrEmpty(newUsername) && string.Equals(username, newUsername, StringComparison.Ordinal))
        {
            throw new ConflictException("Novo uporabniško ime ne sme biti enako prejšnjemu!");
        }

        if (!string.IsNullOrEmpty(newEmail) &&
            string.Equals(email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("Nov e-poštni naslov ne sme biti enak prejšnjemu!");
        }
        
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
    
    [HttpPost("setPfp"), Authorize]
    public async Task<ActionResult<string>> UpdatePfp([FromForm] UpdatePfpDto updatePfpDto)
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

    [HttpPost("updatePassword"), Authorize]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto)
    {
        var username = User.FindFirstValue("username");
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized();

        if (updatePasswordDto.NewPassword == updatePasswordDto.OldPassword)
            throw new ArgumentException("Novo geslo ne sme biti enako prejšnjemu.");

        if (!await authService.VerifyLogin(username, updatePasswordDto.OldPassword))
            throw new UnauthorizedAccessException("Geslo ni pravilno!");

        await userService.UpdatePassword(username, updatePasswordDto.NewPassword);
        return Ok();
    }

    [HttpDelete("deleteUser"), Authorize]
    public async Task<ActionResult> DeleteUser([FromBody] DeleteUserDto deleteUserDto)
    {
        var username = User.FindFirstValue("username");
        if (username == null) return Unauthorized();

        if (!await authService.VerifyLogin(username, deleteUserDto.Password))
            throw new UnauthorizedAccessException("Geslo ni pravilno!");

        await userService.DeleteUser(username);
        return Ok();
    }

    [HttpPost("submitUploaderRequest"), Authorize]
    public async Task<ActionResult> SubmitUploaderRequest([FromBody] UploaderRequestDto uploaderRequestDto)
    {
        var username = User.FindFirstValue("username");
        if (username == null) return Unauthorized();

        if (uploaderRequestDto.Experience.Length > 1000 || uploaderRequestDto.Content.Length > 1000 ||
            uploaderRequestDto.Proof?.Length > 3000 || uploaderRequestDto.OtherTrackers?.Length > 1000)
            throw new ArgumentException("Maksimalna dolžina presežena!");

        if (await userService.IsUploader(username) || await userService.IsAdmin(username))
            throw new InvalidOperationException("Uporabnik je že uploader ali admin!");

        // TODO: Save uploader request in DB
        return Ok();
    }
}
