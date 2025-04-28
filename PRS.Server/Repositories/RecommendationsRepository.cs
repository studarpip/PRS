using PRS.Model.Entities;
using PRS.Server.Repositories.Interfaces;

namespace PRS.Server.Repositories
{
    public class RecommendationsRepository : IRecommendationsRepository
    {
        public async Task<List<Product>> GetRecommendationsAsync(string userId, string context, RecommendationSettings settings)
        {
            var products = new List<Product>();
            return products;
        }
    }
}
