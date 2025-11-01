using PotegniMe.DTOs.Info;
using Microsoft.AspNetCore.Authorization;

namespace PotegniMe.Controllers
{
    [Route("info")]
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
