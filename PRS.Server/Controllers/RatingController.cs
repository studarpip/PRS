using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Services.Interfaces;
using System.Security.Claims;

namespace PRS.Server.Controllers
{
    [ApiController]
    [Route("api/rating")]
    [Authorize]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet("canRate/{productId}")]
        public async Task<ActionResult<ServerResponse<RatingCheckResponse>>> CanRate(Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var response = await _ratingService.CanRateAsync(userId, productId);
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ServerResponse>> SubmitRating([FromBody] RatingRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Unauthorized();

            var response = await _ratingService.SubmitRatingAsync(userId, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
