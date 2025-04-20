using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Services.Interfaces;

namespace PRS.Server.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ServerResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request, HttpContext);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ServerResponse>> Logout()
        {
            await _authService.LogoutAsync(HttpContext);
            return Ok(ServerResponse.Ok());
        }

        [Authorize]
        [HttpGet("currentUser")]
        public ActionResult<ServerResponse<CurrentUserResponse>> CurrentUser()
        {
            var result = _authService.GetCurrentUser(User);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
