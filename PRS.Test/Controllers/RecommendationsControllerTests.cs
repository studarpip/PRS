using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PRS.Model.Entities;
using PRS.Model.Responses;
using PRS.Server.Controllers;
using PRS.Server.Services.Interfaces;
using System.Security.Claims;

public class RecommendationsControllerTests
{
    private readonly Mock<IRecommendationsService> _recommendationServiceMock;
    private readonly RecommendationsController _controller;
    private readonly string _userId;

    public RecommendationsControllerTests()
    {
        _recommendationServiceMock = new Mock<IRecommendationsService>();
        _controller = new RecommendationsController(_recommendationServiceMock.Object);

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
    public async Task GetRecommendations_ShouldReturnOk_WhenSuccess()
    {
        var context = "home";
        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Test", Price = 10 },
            new Product { Id = Guid.NewGuid(), Name = "Item 2", Price = 20 }
        };

        _recommendationServiceMock
            .Setup(x => x.GetRecommendationsAsync(_userId, context))
            .ReturnsAsync(ServerResponse<List<Product>>.Ok(products));

        var result = await _controller.GetRecommendations(context);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();

        var response = ok!.Value as ServerResponse<List<Product>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task GetRecommendations_ShouldReturnUnauthorized_WhenUserIdIsMissing()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.GetRecommendations("home");

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetRecommendations_ShouldReturnBadRequest_WhenServiceFails()
    {
        _recommendationServiceMock
            .Setup(x => x.GetRecommendationsAsync(_userId, "home"))
            .ReturnsAsync(ServerResponse<List<Product>>.Fail("something went wrong"));

        var result = await _controller.GetRecommendations("home");

        var bad = result.Result as BadRequestObjectResult;
        bad.Should().NotBeNull();

        var response = bad!.Value as ServerResponse<List<Product>>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("something went wrong");
    }
}
