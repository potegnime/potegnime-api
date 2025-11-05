using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PotegniMe.DTOs.Recommend;
using PotegniMe.Services.RecommendService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Controllers
{
    [Route("recommend")]
    [ApiController]
    public class RecommendController : ControllerBase
    {
        // Fields
        private readonly IRecommendService _recommendService;
        private readonly IUserService _userService;

        // Constructor
        public RecommendController(IRecommendService recommendService, IUserService userService)
        {
            _recommendService = recommendService;
            _userService = userService;
        }

        // Routes
        [HttpPost, Authorize]
        public async Task<ActionResult<Recommendation>> SetRecommendation([FromBody] Recommendation recommendation)
        {
            // Check if user is admin
            var userId = User.FindFirstValue("uid");
            if (userId == null) return Unauthorized();

            if (!await _userService.IsAdmin(Convert.ToInt32(userId)))
            {
                return Unauthorized();
            }

            try
            {
                recommendation.Type = recommendation.Type.ToLower();
                if (recommendation.Type != "movie" && recommendation.Type != "series")
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Tip mora imeti vrednost movie ali series" });
                }
                Recommendation result = await _recommendService.SetRecommendation(recommendation);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet, Authorize]
        public async Task<ActionResult<Recommendation>> GetRecommendation(DateOnly date, string type)
        {
            try
            {
                type = type.ToLower();
                if (type != "movie" && type != "series")
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Tip mora imeti vrednost movie ali series" });
                }
                Recommendation result = await _recommendService.GetRecommendation(date, type);
                return Ok(result);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpDelete, Authorize]
        public async Task<ActionResult<Recommendation>> DeleteRecommendation(DateOnly date, string type)
        {
            try
            {
                type = type.ToLower();
                if (type != "movie" && type != "series")
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Tip mora imeti vrednost movie ali series" });
                }
                await _recommendService.DeleteRecommendation(date, type);
                return Ok();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("random"), Authorize]
        public async Task<ActionResult<Recommendation>> RandomRecommendation()
        {
            try
            {
                Recommendation result = await _recommendService.RandomRecommendation();
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("nowPlaying"), Authorize]
        public async Task<ActionResult<List<TmdbMovieResponse>>> NowPlaying(string language, int page, string region)
        {
            try
            {
                if (language != "sl-SI" && language != "en-US")
                {
                    return BadRequest();
                }

                var result = await _recommendService.NowPlaying(language, page, region);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("popular"), Authorize]
        public async Task<ActionResult<List<TmdbMovieResponse>>> Popular(string language, int page, string region)
        {
            try
            {
                if (language != "sl-SI" && language != "en-US")
                {
                    return BadRequest();
                }

                var result = await _recommendService.Popular(language, page, region);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("topRated"), Authorize]
        public async Task<ActionResult<List<TmdbMovieResponse>>> TopRated(string language, int page, string region)
        {
            try
            {
                if (language != "sl-SI" && language != "en-US")
                {
                    return BadRequest();
                }

                var result = await _recommendService.TopRated(language, page, region);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("upcoming"), Authorize]
        public async Task<ActionResult<List<TmdbMovieResponse>>> Upcoming(string language, int page, string region)
        {
            try
            {
                if (language != "sl-SI" && language != "en-US")
                {
                    return BadRequest();
                }

                var result = await _recommendService.Upcoming(language, page, region);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("trendingMovie"), Authorize]
        public async Task<ActionResult<List<TmdbTrendingResponse>>> TrendingMovie(string language)
        {
            try
            {
                if (language != "sl-SI" && language != "en-US")
                {
                    return BadRequest();
                }

                var result = await _recommendService.TrendingMovie(language);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("trendingTv"), Authorize]
        public async Task<ActionResult<List<TmdbTrendingResponse>>> TrendingTv(string language)
        {
            try
            {
                if (language != "sl-SI" && language != "en-US")
                {
                    return BadRequest();
                }

                var result = await _recommendService.TrendingTv(language);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }
    }
}