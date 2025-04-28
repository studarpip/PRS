
using PRS.Model.Entities;
using PRS.Model.Responses;

namespace PRS.Server.Services.Interfaces
{
    public interface IRecommendationsService
    {
        Task<ServerResponse<List<Product>>> GetRecommendationsAsync(string userId, string context);
    }
}
