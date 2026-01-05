using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PotegniMe.Core.Exceptions;
using PotegniMe.DTOs.Application;
using PotegniMe.DTOs.User;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers;

[Route("application")]
[ApiController]
public class ApplicationController(DataContext context, IUserService userService) : ControllerBase
{
    [HttpGet, Authorize]
    public async Task<ActionResult<ApplicationDataDto>> GetApplicationData()
    {
        User user = await  GetCurrentUserAsync();
        
        // update users IP
        string? ip = null;
        if (HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var forwardedIp = forwarded.FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedIp))
            {
                ip = forwardedIp.Split(',').First().Trim();
            }
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        if (user.IpAddr != ip && !string.IsNullOrEmpty(ip))
        {
            await userService.UpdateIp(user.Username, ip);
        }
        
        UserDetailsDto userDetails = new UserDetailsDto()
        {
            Username =  user.Username,
            Email = user.Email,
            Joined = DateOnly.FromDateTime(user.JoinedDate),
            Role = user.Role.Name,
            HasPfp = user.ProfilePicFilePath != null
        };

        return Ok(new ApplicationDataDto()
        {
            User = userDetails,
            Language = user.Language
        });
    }
    
    // Helpers
    private async Task<User> GetCurrentUserAsync()
    {
        var uid = User.FindFirstValue("uid");
        if (uid == null) throw new UnauthorizedException("Unauthorized");
        User user = await userService.GetUserById(Convert.ToInt32(uid));
        await context.Entry(user).Reference(u => u.Role).LoadAsync();
        return user;
    }
}