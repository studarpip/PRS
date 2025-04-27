using Microsoft.AspNetCore.Mvc;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Services.Interfaces;

namespace PRS.Server.Controllers
{
    [ApiController]
    [Route("api/register")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost]
        public async Task<ActionResult<ServerResponse>> Register([FromBody] RegistrationRequest request)
        {
            var response = await _registrationService.RegisterAsync(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
