using PRS.Model.Requests;
using PRS.Model.Responses;

namespace PRS.Server.Services.Interfaces
{
    public interface IRatingService
    {
        Task<ServerResponse<RatingCheckResponse>> CanRateAsync(string userId, Guid productId);
        Task<ServerResponse> SubmitRatingAsync(string userId, RatingRequest request);
    }
}
