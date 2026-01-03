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
    [HttpGet, Authorize]
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

        User user = await GetCurrentUserAsync();
        
        // check if there is data to update
        if (string.IsNullOrWhiteSpace(updateUserDto.Username) && string.IsNullOrWhiteSpace(updateUserDto.Email))
        {
            throw new ArgumentException("Ni podatkov za posodobitev.");
        }
        
        var newUsername = updateUserDto.Username?.Trim();
        var newEmail = updateUserDto.Email?.Trim();
        
        if (!string.IsNullOrEmpty(newUsername) && string.Equals(user.Username, newUsername, StringComparison.Ordinal))
        {
            throw new ConflictException("Novo uporabniško ime ne sme biti enako prejšnjemu!");
        }

        if (!string.IsNullOrEmpty(newEmail) &&
            string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("Nov e-poštni naslov ne sme biti enak prejšnjemu!");
        }
        
        if (!string.IsNullOrEmpty(newEmail)) await userService.UpdateEmail(user.Username, newEmail);
        if (!string.IsNullOrEmpty(newUsername))
        {
            await userService.UpdateUsername(user.Username, newUsername);
            // update pfp as well - pfp name is username (https://api.potegni.me/pfp/username)
            await userService.RenamePfp(user.Username, newUsername);
        }
            
        // return new JWT (with new username/email)
        string accessToken = authService.GenerateAccessToken(user);
        return Ok(new JwtTokenResponseDto { AccessToken = accessToken });
    }
    
    [HttpPost("setPfp"), Authorize]
    public async Task<ActionResult<string>> UpdatePfp([FromForm] UpdatePfpDto updatePfpDto)
    {
        // Check if user exists
        var user = await GetCurrentUserAsync();

        // Check if profile picture is attached - determine between update and remove profile picture
        if (updatePfpDto.ProfilePicFile == null)
        {
            // Remove profile picture
            await userService.RemovePfp(user.Username);
            return Ok(new JwtTokenResponseDto
            {
                AccessToken = authService.GenerateAccessToken(user)
            });
        }
        // Update profile picture
        await userService.UpdatePfp(user.Username, updatePfpDto.ProfilePicFile);
        
        return Ok(new JwtTokenResponseDto
        {
            AccessToken = authService.GenerateAccessToken(user)
        });
    }

    [HttpPost("updatePassword"), Authorize]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto)
    {
        User user = await GetCurrentUserAsync();

        if (updatePasswordDto.NewPassword == updatePasswordDto.OldPassword)
            throw new ArgumentException("Novo geslo ne sme biti enako prejšnjemu.");

        if (!await authService.VerifyLogin(user.Username, updatePasswordDto.OldPassword))
            throw new UnauthorizedAccessException("Geslo ni pravilno!");

        await userService.UpdatePassword(user.Username, updatePasswordDto.NewPassword);
        return Ok();
    }

    [HttpDelete("deleteUser"), Authorize]
    public async Task<ActionResult> DeleteUser([FromBody] DeleteUserDto deleteUserDto)
    {
        User user = await GetCurrentUserAsync();

        if (!await authService.VerifyLogin(user.Username, deleteUserDto.Password))
            throw new UnauthorizedAccessException("Geslo ni pravilno!");

        await userService.DeleteUser(user.Username);
        return Ok();
    }

    [HttpPost("submitUploaderRequest"), Authorize]
    public async Task<ActionResult> SubmitUploaderRequest([FromBody] UploaderRequestDto uploaderRequestDto)
    {
        User user =  await GetCurrentUserAsync();

        if (uploaderRequestDto.Experience.Length > 1000 || uploaderRequestDto.Content.Length > 1000 ||
            uploaderRequestDto.Proof?.Length > 3000 || uploaderRequestDto.OtherTrackers?.Length > 1000)
            throw new ArgumentException("Maksimalna dolžina presežena!");

        if (await userService.IsUploader(user.Username) || await userService.IsAdmin(user.Username))
            throw new InvalidOperationException("Uporabnik je že uploader ali admin!");

        // TODO: Save uploader request in DB
        return Ok();
    }
    
    // Helpers
    private async Task<User> GetCurrentUserAsync()
    {
        var uid = User.FindFirstValue("uid");
        if (uid == null) throw new UnauthorizedException("Unauthorized");

        return await userService.GetUserById(Convert.ToInt32(uid));
    }
}
