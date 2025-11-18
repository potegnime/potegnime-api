using Microsoft.AspNetCore.Authorization;

namespace PotegniMe.Controllers
{
    [Route("")]
    [ApiController]
    public class RootController : ControllerBase
    {

        // Constructor
        public RootController()
        {
        }

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult> Ping()
        {
            return StatusCode(200, "api" );
        }
    }
}
