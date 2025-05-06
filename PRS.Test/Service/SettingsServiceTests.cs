using FluentAssertions;
using Moq;
using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services;

public class SettingsServiceTests
{
    private readonly Mock<ISettingsRepository> _settingsRepositoryMock;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _settingsRepositoryMock = new Mock<ISettingsRepository>();
        _service = new SettingsService(_settingsRepositoryMock.Object);
    }

    [Fact]
    public async Task GetSettingsAsync_ShouldReturnSettings()
    {
        var userId = "user-123";
        var expectedSettings = new RecommendationSettings
        {
            UseContent = true,
            UseCollaborative = true,
            CategoryWeight = 0.8,
            PriceWeight = 0.2,
            BrowseWeight = 0.1,
            ViewWeight = 0.2,
            CartWeight = 0.3,
            PurchaseWeight = 0.3,
            RatingWeight = 0.1,
            AvgRatingWeight = 0.1
        };

        _settingsRepositoryMock.Setup(r => r.GetSettingsAsync(userId))
            .ReturnsAsync(expectedSettings);

        var result = await _service.GetSettingsAsync(userId);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedSettings);
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldCallRepository()
    {
        var userId = "user-123";
        var request = new RecommendationSettingRequest
        {
            UseContent = false,
            UseCollaborative = true,
            CategoryWeight = 1.0,
            PriceWeight = 0.5,
            BrowseWeight = 0.3,
            ViewWeight = 0.4,
            CartWeight = 0.2,
            PurchaseWeight = 0.9,
            RatingWeight = 0.7,
            AvgRatingWeight = 0.6
        };

        var result = await _service.SaveSettingsAsync(userId, request);

        result.Success.Should().BeTrue();

        _settingsRepositoryMock.Verify(r => r.SaveSettingsAsync(userId, request), Times.Once);
    }
}
