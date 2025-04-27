using PRS.Model.Entities;
using PRS.Model.Requests;

namespace PRS.Server.Repositories.Interfaces
{
    public interface ISettingsRepository
    {
        Task<RecommendationSettings> GetSettingsAsync(string userId);
        Task SaveSettingsAsync(string userId, RecommendationSettingRequest request);
    }
}
