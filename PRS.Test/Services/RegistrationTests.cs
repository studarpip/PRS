using FluentAssertions;
using Moq;
using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Server.Helpers;
using PRS.Server.Helpers.Interfaces;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services;

public class RegistrationServiceTests
{
    private readonly Mock<IRegistrationRepository> _repositoryMock;
    private readonly Mock<IEncryptionHelper> _encryptionMock;
    private readonly RegistrationService _service;

    public RegistrationServiceTests()
    {
        _repositoryMock = new Mock<IRegistrationRepository>();
        _encryptionMock = new Mock<IEncryptionHelper>();

        _service = new RegistrationService(_repositoryMock.Object, _encryptionMock.Object);
    }

    private RegistrationRequest ValidRequest => new RegistrationRequest
    {
        Username = "TestUser",
        Password = "Strong123!",
        Email = "test@example.com",
        Gender = Gender.Male,
        Country = Country.Lithuania,
        DateOfBirth = new DateTime(2000, 1, 1)
    };

    [Fact]
    public async Task RegisterAsync_ShouldThrow_IfPasswordIsWeak()
    {
        var request = ValidRequest;
        request.Password = "123";

        Func<Task> act = async () => await _service.RegisterAsync(request);

        await act.Should().ThrowAsync<WeakPasswordException>();
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_IfEmailInvalid()
    {
        var request = ValidRequest;
        request.Email = "invalid_email";

        Func<Task> act = async () => await _service.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidEmailFormatException>();
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_IfUsernameExists()
    {
        var request = ValidRequest;

        _repositoryMock.Setup(r => r.UsernameExistsAsync(request.Username))
                       .ReturnsAsync(true);

        Func<Task> act = async () => await _service.RegisterAsync(request);

        await act.Should().ThrowAsync<UsernameAlreadyExistsException>();
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_IfEmailExists()
    {
        var request = ValidRequest;

        _repositoryMock.Setup(r => r.UsernameExistsAsync(request.Username)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        Func<Task> act = async () => await _service.RegisterAsync(request);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
    }

    [Fact]
    public async Task RegisterAsync_ShouldSucceed_WhenValid()
    {
        var request = ValidRequest;

        _repositoryMock.Setup(r => r.UsernameExistsAsync(request.Username)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _encryptionMock.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(s => $"encrypted-{s}");

        var result = await _service.RegisterAsync(request);

        result.Success.Should().BeTrue();

        _repositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u =>
            u.Username == request.Username &&
            u.Email.StartsWith("encrypted-") &&
            u.EmailHash == request.Email.ToLower().HashString() &&
            u.Password == request.Password.HashString() &&
            u.Country == Country.Lithuania &&
            u.Gender == Gender.Male
        )), Times.Once);
    }
}
