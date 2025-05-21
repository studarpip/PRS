using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Server.Helpers;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services;
using System.Security.Claims;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _repositoryMock;
    private readonly DefaultHttpContext _httpContext;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _repositoryMock = new Mock<IAuthRepository>();

        var authServiceMock = new Mock<IAuthenticationService>();

        authServiceMock
            .Setup(a => a.SignInAsync(It.IsAny<HttpContext>(),
                                      It.IsAny<string>(),
                                      It.IsAny<ClaimsPrincipal>(),
                                      It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        authServiceMock
            .Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(),
                                       It.IsAny<string>(),
                                       It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(authServiceMock.Object)
            .BuildServiceProvider();

        _httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        _authService = new AuthService(_repositoryMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var user = new UserBasic
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Password = "password123".HashString(),
            Role = Role.User
        };

        var request = new LoginRequest
        {
            Username = "test",
            Password = "password123"
        };

        _repositoryMock.Setup(r => r.GetByUsernameAsync("test"))
                       .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request, _httpContext);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
    {
        _repositoryMock.Setup(r => r.GetByUsernameAsync("nonexistent"))
                       .ReturnsAsync((UserBasic?)null);

        var act = async () => await _authService.LoginAsync(new LoginRequest
        {
            Username = "nonexistent",
            Password = "irrelevant"
        }, _httpContext);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIncorrect()
    {
        var user = new UserBasic
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Password = "correctpassword".HashString(),
            Role = Role.User
        };

        _repositoryMock.Setup(r => r.GetByUsernameAsync("test"))
                       .ReturnsAsync(user);

        var act = async () => await _authService.LoginAsync(new LoginRequest
        {
            Username = "test",
            Password = "wrongpassword"
        }, _httpContext);

        await act.Should().ThrowAsync<IncorrectPasswordException>();
    }

    [Fact]
    public async Task LogoutAsync_ShouldNotThrow()
    {
        var act = async () => await _authService.LogoutAsync(_httpContext);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void GetCurrentUser_ShouldReturnCorrectUserData()
    {
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "john"),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, Role.Admin.ToString())
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        var result = _authService.GetCurrentUser(principal);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().BeNull();
        result.Data!.Username.Should().Be("john");
        result.Data.UserId.Should().Be(userId);
        result.Data.Role.Should().Be(Role.Admin);
    }
}
