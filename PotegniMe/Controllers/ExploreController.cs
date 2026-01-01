using Microsoft.AspNetCore.Authorization;
using PotegniMe.Services.ExploreService;

namespace PotegniMe.Controllers;

[Route("explore")]
[ApiController]
public class ExploreController(IExploreService exploreService) : ControllerBase
{
    [HttpGet("random"), Authorize]
    public async Task<ActionResult<Recommendation>> RandomRecommendation()
    {
        Recommendation result = await exploreService.RandomRecommendation();
        return Ok(result);
    }

    [HttpGet, Authorize]
    public async Task<ActionResult> Explore(string type, string language, int page = 1, string? region = null)
    {
        if ((type == "trending_movie" || type == "trending_movie") && string.IsNullOrEmpty(region)) 
            throw new ArgumentException("Region is required!!");
        
        return type.ToLower() switch
        {
            "now_playing" => Ok(await exploreService.NowPlaying(language, page, region!)),
            "popular" => Ok(await exploreService.Popular(language, page, region!)),
            "top_rated" => Ok(await exploreService.TopRated(language, page, region!)),
            "upcoming" => Ok(await exploreService.Upcoming(language, page, region!)),
            "trending_movie" => Ok(await exploreService.TrendingMovie(language)),
            "trending_tv" => Ok(await exploreService.TrendingTv(language)),
            _ => throw new ArgumentException("Invalid explore type")
        };
    }
}
