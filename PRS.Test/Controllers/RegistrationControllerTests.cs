using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PRS.Model.Enums;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Controllers;
using PRS.Server.Services.Interfaces;

public class RegistrationControllerTests
{
    private readonly Mock<IRegistrationService> _registrationServiceMock;
    private readonly RegistrationController _controller;

    public RegistrationControllerTests()
    {
        _registrationServiceMock = new Mock<IRegistrationService>();
        _controller = new RegistrationController(_registrationServiceMock.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
    {
        var request = CreateValidRequest();

        _registrationServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(ServerResponse.Ok());

        var result = await _controller.Register(request);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ((ServerResponse)ok!.Value!).Success.Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenServiceReturnsFailResponse()
    {
        var request = CreateValidRequest();

        var failResponse = ServerResponse.Fail("Error occurred");
        _registrationServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(failResponse);

        var result = await _controller.Register(request);

        var bad = result.Result as BadRequestObjectResult;
        bad.Should().NotBeNull();
        ((ServerResponse)bad!.Value!).Success.Should().BeFalse();
    }

    [Fact]
    public async Task Register_ShouldThrowWeakPasswordException_WhenPasswordIsWeak()
    {
        var request = CreateValidRequest();
        _registrationServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new WeakPasswordException());

        Func<Task> act = async () => await _controller.Register(request);
        await act.Should().ThrowAsync<WeakPasswordException>();
    }

    [Fact]
    public async Task Register_ShouldThrowUsernameAlreadyExistsException_WhenUsernameExists()
    {
        var request = CreateValidRequest();
        _registrationServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new UsernameAlreadyExistsException());

        Func<Task> act = async () => await _controller.Register(request);
        await act.Should().ThrowAsync<UsernameAlreadyExistsException>();
    }

    private RegistrationRequest CreateValidRequest()
    {
        return new RegistrationRequest
        {
            Username = "newUser",
            Email = "user@example.com",
            Password = "StrongP@ss1!",
            Gender = Gender.Male,
            Country = Country.Lithuania,
            DateOfBirth = DateTime.Today.AddYears(-20)
        };
    }
}
