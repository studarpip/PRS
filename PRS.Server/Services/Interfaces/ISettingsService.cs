using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;

namespace PRS.Server.Services.Interfaces
{
    public interface ISettingsService
    {
        Task<ServerResponse<RecommendationSettings>> GetSettingsAsync(string userId);
        Task<ServerResponse> SaveSettingsAsync(string userId, RecommendationSettingRequest request);
    }
}
