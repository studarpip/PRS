using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PRS.Model.Enums;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Helpers;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services.Interfaces;
using System.Security.Claims;

namespace PRS.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;

        public AuthService(IAuthRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServerResponse> LoginAsync(LoginRequest request, HttpContext httpContext)
        {
            var user = await _repository.GetByUsernameAsync(request.Username)
                        ?? throw new UserNotFoundException();

            if (!request.Password.IsStringHashedMatch(user.Password))
                throw new IncorrectPasswordException();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return ServerResponse.Ok();
        }

        public async Task LogoutAsync(HttpContext httpContext) => await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        public ServerResponse<CurrentUserResponse> GetCurrentUser(ClaimsPrincipal user)
        {
            var username = user.Identity!.Name!;
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = Enum.Parse<Role>(user.FindFirst(ClaimTypes.Role)!.Value);

            return ServerResponse<CurrentUserResponse>.Ok(new CurrentUserResponse
            {
                Username = username,
                UserId = userId,
                Role = role
            });
        }
    }
}
