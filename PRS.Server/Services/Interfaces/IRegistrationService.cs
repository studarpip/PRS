using PRS.Model.Requests;
using PRS.Model.Responses;

namespace PRS.Server.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<ServerResponse> RegisterAsync(RegistrationRequest request);
    }
}
