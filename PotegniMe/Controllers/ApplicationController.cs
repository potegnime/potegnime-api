using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PotegniMe.DTOs.Application;
using PotegniMe.DTOs.User;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers;

[Route("application")]
[ApiController]
public class ApplicationController(IUserService userService) : ControllerBase
{
    [HttpGet, Authorize]
    public async Task<ActionResult<ApplicationDataDto>> GetApplicationData()
    {
        var username = User.FindFirstValue("username");
        if (string.IsNullOrWhiteSpace(username)) return Unauthorized();
        var user = await userService.GetUserByUsername(username);

        UserDetailsDto userDetails = new UserDetailsDto()
        {
            Username =  user.Username,
            Email = user.Email,
            Joined = user.JoinedDate,
            Role = user.Role.Name,
            HasPfp = user.ProfilePicFilePath != null
        };

        return Ok(new ApplicationDataDto()
        {
            User = userDetails,
            Language = user.Language
        });
    }
}