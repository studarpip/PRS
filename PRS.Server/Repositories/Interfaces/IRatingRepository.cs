using PRS.Model.Responses;

namespace PRS.Server.Repositories.Interfaces
{
    public interface IRatingRepository
    {
        Task<RatingCheckResponse> CheckIfUserCanRateAsync(string userId, Guid productId);
        Task SubmitRatingAsync(string userId, Guid productId, int rating);
    }
}
