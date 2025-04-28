using PRS.Model.Entities;
using PRS.Model.Responses;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services.Interfaces;

namespace PRS.Server.Services
{
    public class RecommendationsService : IRecommendationsService
    {
        private readonly IRecommendationsRepository _recommendationsRepository;
        private readonly ISettingsRepository _settingsRepository;

        public RecommendationsService(IRecommendationsRepository recommendationsRepository, ISettingsRepository settingsRepository)
        {
            _recommendationsRepository = recommendationsRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task<ServerResponse<List<Product>>> GetRecommendationsAsync(string userId, string context)
        {
            var settings = await _settingsRepository.GetSettingsAsync(userId);
            var products = await _recommendationsRepository.GetRecommendationsAsync(userId, context, settings);
            return ServerResponse<List<Product>>.Ok(products);
        }
    }
}
