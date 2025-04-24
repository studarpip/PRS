using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services.Interfaces;

namespace PRS.Server.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;

        public RatingService(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository;
        }

        public async Task<ServerResponse<RatingCheckResponse>> CanRateAsync(string userId, Guid productId)
        {
            var result = await _ratingRepository.CheckIfUserCanRateAsync(userId, productId);
            return ServerResponse<RatingCheckResponse>.Ok(result);
        }

        public async Task<ServerResponse> SubmitRatingAsync(string userId, RatingRequest request)
        {
            var canRate = await _ratingRepository.CheckIfUserCanRateAsync(userId, request.ProductId);
            if (!canRate.CanRate)
                throw new CannotRateProductException();

            await _ratingRepository.SubmitRatingAsync(userId, request.ProductId, request.Rating);
            return ServerResponse.Ok();
        }
    }
}
