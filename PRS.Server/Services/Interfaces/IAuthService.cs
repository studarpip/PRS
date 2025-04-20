using PRS.Model.Requests;
using PRS.Model.Responses;
using System.Security.Claims;

namespace PRS.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServerResponse> LoginAsync(LoginRequest request, HttpContext httpContext);
        Task LogoutAsync(HttpContext httpContext);
        ServerResponse<CurrentUserResponse> GetCurrentUser(ClaimsPrincipal user);
    }

}
