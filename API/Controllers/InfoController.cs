using API.DTOs.Info;
using API.DTOs.Search;
using API.DTOs.TorrentScrape;
using API.Services.SearchService;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/info")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        // Fields
        private readonly IConfiguration _configuration;

        // Constructor
        public InfoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult> Info()
        {
            return StatusCode(200, new InfoResponseDto { Response = "API is running"});
        }
    }
}
