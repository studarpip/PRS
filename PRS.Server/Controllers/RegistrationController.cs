using Microsoft.AspNetCore.Mvc;
using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Helpers;
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

        [HttpGet("options")]
        public ActionResult<ServerResponse<RegistrationOptionsResponse>> GetOptions()
        {
            var genders = Enum.GetValues<Gender>()
                .Where(g => !g.ShouldSkipRelationship())
                .Select(g => new EnumOption { Value = (int)g, Label = g.ToString() })
                .ToList();

            var countries = Enum.GetValues<Country>()
                .Where(c => !c.ShouldSkipRelationship())
                .Select(c => new EnumOption { Value = (int)c, Label = c.ToString() })
                .ToList();

            var response = new RegistrationOptionsResponse
            {
                Genders = genders,
                Countries = countries
            };

            return Ok(ServerResponse<RegistrationOptionsResponse>.Ok(response));
        }
    }
}
