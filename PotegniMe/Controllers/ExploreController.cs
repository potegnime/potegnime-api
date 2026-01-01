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
    public async Task<ActionResult> Explore(string types, string language, int page = 1, string? region = null)
    {
        if (string.IsNullOrEmpty(types)) throw new ArgumentException("At least one type must be specified");

        var typeList = types.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim().ToLower()).ToList();
        var result = new Dictionary<string, object>();

        foreach (var type in typeList)
        {
            switch (type)
            {
                case "now_playing":
                    if (string.IsNullOrEmpty(region)) throw new ArgumentException("Region is required for now_playing");
                    result[type] = await exploreService.NowPlaying(language, page, region);
                    break;
                case "popular":
                    if (string.IsNullOrEmpty(region)) throw new ArgumentException("Region is required for popular");
                    result[type] = await exploreService.Popular(language, page, region);
                    break;
                case "top_rated":
                    if (string.IsNullOrEmpty(region)) throw new ArgumentException("Region is required for top_rated");
                    result[type] = await exploreService.TopRated(language, page, region);
                    break;
                case "upcoming":
                    if (string.IsNullOrEmpty(region)) throw new ArgumentException("Region is required for upcoming");
                    result[type] = await exploreService.Upcoming(language, page, region);
                    break;
                case "trending_movie":
                    result[type] = await exploreService.TrendingMovie(language);
                    break;
                case "trending_tv":
                    result[type] = await exploreService.TrendingTv(language);
                    break;
                default:
                    throw new ArgumentException($"Invalid explore type: {type}");
            }
        }

        return Ok(result);
    }
}
