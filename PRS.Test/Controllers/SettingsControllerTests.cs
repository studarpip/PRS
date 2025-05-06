using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Controllers;
using PRS.Server.Services.Interfaces;
using System.Security.Claims;

public class SettingsControllerTests
{
    private readonly Mock<ISettingsService> _serviceMock;
    private readonly SettingsController _controller;

    public SettingsControllerTests()
    {
        _serviceMock = new Mock<ISettingsService>();
        _controller = new SettingsController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetSettings_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.GetSettings();
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetSettings_ShouldReturnOk_WhenServiceReturnsSuccess()
    {
        var userId = Guid.NewGuid().ToString();
        _controller.ControllerContext = BuildContextWithUserId(userId);

        var settings = new RecommendationSettings { UseContent = true, UseCollaborative = false };
        var response = ServerResponse<RecommendationSettings>.Ok(settings);

        _serviceMock.Setup(s => s.GetSettingsAsync(userId)).ReturnsAsync(response);

        var result = await _controller.GetSettings();

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task SaveSettings_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.SaveSettings(new RecommendationSettingRequest());
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task SaveSettings_ShouldReturnOk_WhenSuccess()
    {
        var userId = Guid.NewGuid().ToString();
        _controller.ControllerContext = BuildContextWithUserId(userId);

        var request = new RecommendationSettingRequest { UseContent = true, UseCollaborative = true };
        _serviceMock.Setup(s => s.SaveSettingsAsync(userId, request)).ReturnsAsync(ServerResponse.Ok());

        var result = await _controller.SaveSettings(request);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ((ServerResponse)ok!.Value!).Success.Should().BeTrue();
    }

    [Fact]
    public async Task SaveSettings_ShouldReturnBadRequest_WhenFail()
    {
        var userId = Guid.NewGuid().ToString();
        _controller.ControllerContext = BuildContextWithUserId(userId);

        var request = new RecommendationSettingRequest();
        var fail = ServerResponse.Fail("Error");
        _serviceMock.Setup(s => s.SaveSettingsAsync(userId, request)).ReturnsAsync(fail);

        var result = await _controller.SaveSettings(request);

        var bad = result.Result as BadRequestObjectResult;
        bad.Should().NotBeNull();
        ((ServerResponse)bad!.Value!).Success.Should().BeFalse();
    }

    private ControllerContext BuildContextWithUserId(string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
    }
}
