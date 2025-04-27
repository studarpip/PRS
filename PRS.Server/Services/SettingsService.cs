using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services.Interfaces;

namespace PRS.Server.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<ServerResponse<RecommendationSettings>> GetSettingsAsync(string userId)
        {
            var settings = await _settingsRepository.GetSettingsAsync(userId);
            return ServerResponse<RecommendationSettings>.Ok(settings);
        }

        public async Task<ServerResponse> SaveSettingsAsync(string userId, RecommendationSettingRequest request)
        {
            await _settingsRepository.SaveSettingsAsync(userId, request);
            return ServerResponse.Ok();
        }
    }
}
