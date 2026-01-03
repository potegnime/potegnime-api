using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PotegniMe.Core.Exceptions;
using PotegniMe.Services.RecommendService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers;

[Route("recommend")]
[ApiController]
public class RecommendController(IRecommendService recommendService, IUserService userService) : ControllerBase
{

    // Routes
    [HttpPost, Authorize]
    public async Task<ActionResult<Recommendation>> SetRecommendation([FromBody] Recommendation recommendation)
    {
        // Check if user is admin
        var user = await  GetCurrentUserAsync();
        if (!await userService.IsAdmin(user.Username)) return Unauthorized();
        
        Recommendation result = await recommendService.SetRecommendation(recommendation);
        return Ok(result);
    }

    [HttpGet, Authorize]
    public async Task<ActionResult<Recommendation>> GetRecommendation(DateOnly date, string type)
    {
        Recommendation result = await recommendService.GetRecommendation(date, type);
        return Ok(result);
    }

    [HttpDelete, Authorize]
    public async Task<ActionResult<Recommendation>> DeleteRecommendation(DateOnly date, string type)
    {
        var user = await GetCurrentUserAsync();
        if (!await userService.IsAdmin(user.Username)) return Unauthorized();
        
        await recommendService.DeleteRecommendation(date, type);
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
