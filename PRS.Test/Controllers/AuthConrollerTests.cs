using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PRS.Model.Enums;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Controllers;
using PRS.Server.Services.Interfaces;
using System.Security.Claims;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenSuccessful()
    {
        var request = new LoginRequest { Username = "user", Password = "pass" };
        var context = new DefaultHttpContext();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        _authServiceMock.Setup(x => x.LoginAsync(request, context))
            .ReturnsAsync(ServerResponse.Ok());

        var result = await _controller.Login(request);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeOfType<ServerResponse>();
        ((ServerResponse)ok.Value!).Success.Should().BeTrue();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenFails()
    {
        var request = new LoginRequest { Username = "user", Password = "wrong" };
        var context = new DefaultHttpContext();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        var failResponse = ServerResponse.Fail("Invalid credentials");
        _authServiceMock.Setup(x => x.LoginAsync(request, context))
            .ReturnsAsync(failResponse);

        var result = await _controller.Login(request);

        var unauthorized = result.Result as UnauthorizedObjectResult;
        unauthorized.Should().NotBeNull();
        unauthorized!.Value.Should().Be(failResponse);
    }

    [Fact]
    public async Task Logout_ShouldReturnOk()
    {
        var context = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = context };

        _authServiceMock.Setup(x => x.LogoutAsync(context)).Returns(Task.CompletedTask);

        var result = await _controller.Logout();

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ((ServerResponse)ok!.Value!).Success.Should().BeTrue();
    }

    [Fact]
    public void CurrentUser_ShouldReturnOk_WhenUserValid()
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, Role.Admin.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        _authServiceMock.Setup(x => x.GetCurrentUser(principal))
            .Returns(ServerResponse<CurrentUserResponse>.Ok(new CurrentUserResponse
            {
                Username = "admin",
                UserId = userId,
                Role = Role.Admin
            }));

        var result = _controller.CurrentUser();
        var ok = result.Result as OkObjectResult;

        ok.Should().NotBeNull();
        var response = ok!.Value as ServerResponse<CurrentUserResponse>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.Username.Should().Be("admin");
    }

    [Fact]
    public void CurrentUser_ShouldReturnUnauthorized_WhenFails()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "user"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, Role.User.ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        _authServiceMock.Setup(x => x.GetCurrentUser(principal))
            .Returns(ServerResponse<CurrentUserResponse>.Fail("fail"));

        var result = _controller.CurrentUser();
        var unauthorized = result.Result as UnauthorizedObjectResult;

        unauthorized.Should().NotBeNull();
        var response = unauthorized!.Value as ServerResponse<CurrentUserResponse>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
    }
}
