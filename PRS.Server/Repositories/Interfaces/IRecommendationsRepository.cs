using PRS.Model.Entities;

namespace PRS.Server.Repositories.Interfaces
{
    public interface IRecommendationsRepository
    {
        Task<List<Product>> GetRecommendationsAsync(string userId, string context, RecommendationSettings settings);
    }
}
