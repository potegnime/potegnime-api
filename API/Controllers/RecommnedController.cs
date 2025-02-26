using API.DTOs.Recommend;
using API.Services.RecommendService;
using API.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/recommend")]
    [ApiController]
    public class RecommnedController : ControllerBase
    {
        // Fields
        private readonly IRecommnedService _recommnedService;
        private readonly IUserService _userService;

        // Constructor
        public RecommnedController(IRecommnedService recommnedService, IUserService userService)
        {
            _recommnedService = recommnedService;
            _userService = userService;
        }

        // Routes
        [HttpPost, Authorize]
        public async Task<ActionResult<Recommendation>> SetRecommendation([FromForm] Recommendation recommendation)
        {
            // Check if user is admin
            var userId = User.FindFirstValue("uid");
            if (userId == null) return Unauthorized();
            
            if (!await _userService.IsAdmin(Convert.ToInt32(userId))) {
                return Unauthorized();
            }

            try
            {
                recommendation.Type = recommendation.Type.ToLower();
                if (recommendation.Type != "movie" && recommendation.Type != "series")
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Tip mora imeti vrednost movie ali series" });
                }
                Recommendation result = await _recommnedService.SetRecommendation(recommendation);
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
                Recommendation result = await _recommnedService.GetRecommendation(date, type);
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
                await _recommnedService.DeleteRecommendation(date, type);
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
                Recommendation result = await _recommnedService.RandomRecommendation();
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

                var result = await _recommnedService.NowPlaying(language, page, region);
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

                var result = await _recommnedService.Popular(language, page, region);
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

                var result = await _recommnedService.TopRated(language, page, region);
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

                var result = await _recommnedService.Upcoming(language, page, region);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("trendingMovie"), Authorize]
        public async Task<ActionResult<List<TmdbTrendingResponse>>> TrendingMovie(string timeWindow, string language)
        {
            try
            {
                timeWindow = timeWindow.ToLower();
                if (timeWindow != "day" && timeWindow != "week")
                {
                    return BadRequest();
                }

                if (language != "sl-SI" && language != "en-US")
                {
                    return BadRequest();
                }

                var result = await _recommnedService.TrendingMovie(timeWindow, language);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("trendingTv"), Authorize]
        public async Task<ActionResult<List<TmdbTrendingResponse>>> TrendingTv(string timeWindow, string language)
        {
            try
            {
                timeWindow = timeWindow.ToLower();
                if (timeWindow != "day" && timeWindow != "week")
                {
                    return BadRequest();
                }

                if (language != "sl-SI" && language != "en-US")
                {
                    return BadRequest();
                }

                var result = await _recommnedService.TrendingTv(timeWindow, language);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }
    }
}