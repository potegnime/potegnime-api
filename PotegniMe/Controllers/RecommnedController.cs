using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
        var username = User.FindFirstValue("username");
        if (string.IsNullOrWhiteSpace(username)) return Unauthorized();
        if (!await userService.IsAdmin(username)) return Unauthorized();
        
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
        var username = User.FindFirstValue("username");
        if (string.IsNullOrWhiteSpace(username)) return Unauthorized();
        if (!await userService.IsAdmin(username)) return Unauthorized();
        
        await recommendService.DeleteRecommendation(date, type);
        return Ok();
    }
}
