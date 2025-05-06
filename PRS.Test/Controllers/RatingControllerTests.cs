using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Controllers;
using PRS.Server.Services.Interfaces;
using System.Security.Claims;

public class RatingControllerTests
{
    private readonly Mock<IRatingService> _ratingServiceMock;
    private readonly RatingController _controller;
    private readonly string _userId;

    public RatingControllerTests()
    {
        _ratingServiceMock = new Mock<IRatingService>();
        _controller = new RatingController(_ratingServiceMock.Object);

        _userId = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _userId)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task CanRate_ShouldReturnOk_WhenServiceReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var mockResponse = new RatingCheckResponse { CanRate = true, PreviousRating = null };

        _ratingServiceMock.Setup(x => x.CanRateAsync(_userId, productId))
            .ReturnsAsync(ServerResponse<RatingCheckResponse>.Ok(mockResponse));

        var result = await _controller.CanRate(productId);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var response = ok!.Value as ServerResponse<RatingCheckResponse>;
        response!.Success.Should().BeTrue();
        response.Data.CanRate.Should().BeTrue();
    }

    [Fact]
    public async Task CanRate_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();

        var result = await _controller.CanRate(Guid.NewGuid());
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task SubmitRating_ShouldReturnOk_WhenSuccessful()
    {
        var request = new RatingRequest
        {
            ProductId = Guid.NewGuid(),
            Rating = 5
        };

        _ratingServiceMock.Setup(x => x.SubmitRatingAsync(_userId, request))
            .ReturnsAsync(ServerResponse.Ok());

        var result = await _controller.SubmitRating(request);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ((ServerResponse)ok!.Value!).Success.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitRating_ShouldReturnUnauthorized_WhenUserMissing()
    {
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();

        var result = await _controller.SubmitRating(new RatingRequest());
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task SubmitRating_ShouldReturnBadRequest_WhenFails()
    {
        var request = new RatingRequest
        {
            ProductId = Guid.NewGuid(),
            Rating = 2
        };

        var failResponse = ServerResponse.Fail("Cannot rate");

        _ratingServiceMock.Setup(x => x.SubmitRatingAsync(_userId, request))
            .ReturnsAsync(failResponse);

        var result = await _controller.SubmitRating(request);

        var bad = result.Result as BadRequestObjectResult;
        bad.Should().NotBeNull();
        ((ServerResponse)bad!.Value!).Success.Should().BeFalse();
    }
}
