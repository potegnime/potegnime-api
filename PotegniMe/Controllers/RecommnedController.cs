using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PotegniMe.DTOs.Recommend;
using PotegniMe.Services.RecommendService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers
{
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

        [HttpGet("random"), Authorize]
        public async Task<ActionResult<Recommendation>> RandomRecommendation()
        {
            Recommendation result = await recommendService.RandomRecommendation();
            return Ok(result);
        }

        [HttpGet("nowPlaying"), Authorize]
        public async Task<ActionResult<List<TmdbMovieResponse>>> NowPlaying(string language, int page, string region)
        {
            var result = await recommendService.NowPlaying(language, page, region);
            return Ok(result);
        }

        [HttpGet("popular"), Authorize]
        public async Task<ActionResult<List<TmdbMovieResponse>>> Popular(string language, int page, string region)
        {
            var result = await recommendService.Popular(language, page, region);
            return Ok(result);
        }

        [HttpGet("topRated"), Authorize]
        public async Task<ActionResult<List<TmdbMovieResponse>>> TopRated(string language, int page, string region)
        {
            var result = await recommendService.TopRated(language, page, region);
            return Ok(result);
        }

        [HttpGet("upcoming"), Authorize]
        public async Task<ActionResult<List<TmdbMovieResponse>>> Upcoming(string language, int page, string region)
        {
            var result = await recommendService.Upcoming(language, page, region);
            return Ok(result);
        }

        [HttpGet("trendingMovie"), Authorize]
        public async Task<ActionResult<List<TmdbTrendingResponse>>> TrendingMovie(string language)
        {
            var result = await recommendService.TrendingMovie(language);
            return Ok(result);
        }

        [HttpGet("trendingTv"), Authorize]
        public async Task<ActionResult<List<TmdbTrendingResponse>>> TrendingTv(string language)
        {
            var result = await recommendService.TrendingTv(language);
            return Ok(result);
        }
    }
}