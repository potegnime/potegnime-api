using Microsoft.AspNetCore.Authorization;

namespace PotegniMe.Controllers
{
    [Route("")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet, AllowAnonymous]
        public async Task<ActionResult> Root()
        {
            return StatusCode(200, "api" );
        }
    }
}
