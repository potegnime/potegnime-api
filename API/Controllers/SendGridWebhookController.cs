using API.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Sendgrid.Webhooks.Service;

namespace API.Controllers.SendGrid
{
    [Route("api/sendgrid")]
    [ApiController]
    public class SendGridWebHookController: ControllerBase
    {
        // Fields
        private readonly IAuthService _authService;

        // Constructor
        public SendGridWebHookController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("notify"), AllowAnonymous]
        public async Task<ActionResult> Notify(List<dynamic> events)
        {
            // TODO - rate limit
            // Count number of emails sent from a given IP
            // Count number of emails sent to a given email
            var parser = new WebhookParser();
            return Ok();
        }
    }
}

