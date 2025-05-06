using FluentAssertions;
using Moq;
using PRS.Model.Entities;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services;

public class RecommendationsServiceTests
{
    private readonly Mock<IRecommendationsRepository> _recommendationsRepositoryMock;
    private readonly Mock<ISettingsRepository> _settingsRepositoryMock;
    private readonly RecommendationsService _service;

    public RecommendationsServiceTests()
    {
        _recommendationsRepositoryMock = new Mock<IRecommendationsRepository>();
        _settingsRepositoryMock = new Mock<ISettingsRepository>();

        _service = new RecommendationsService(
            _recommendationsRepositoryMock.Object,
            _settingsRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldReturnProducts()
    {
        var userId = "user-123";
        var context = "home";

        var mockSettings = new RecommendationSettings
        {
            UseContent = true,
            UseCollaborative = false,
            CategoryWeight = 1,
            PriceWeight = 1,
            BrowseWeight = 1,
            ViewWeight = 1,
            CartWeight = 1,
            PurchaseWeight = 1,
            RatingWeight = 1,
            AvgRatingWeight = 1
        };

        var recommendedProducts = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "P1", Price = 10 },
            new Product { Id = Guid.NewGuid(), Name = "P2", Price = 20 }
        };

        _settingsRepositoryMock.Setup(r => r.GetSettingsAsync(userId))
            .ReturnsAsync(mockSettings);

        _recommendationsRepositoryMock.Setup(r =>
                r.GetRecommendationsAsync(userId, context, mockSettings))
            .ReturnsAsync(recommendedProducts);

        var result = await _service.GetRecommendationsAsync(userId, context);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(recommendedProducts);

        _settingsRepositoryMock.Verify(r => r.GetSettingsAsync(userId), Times.Once);
        _recommendationsRepositoryMock.Verify(r =>
            r.GetRecommendationsAsync(userId, context, mockSettings), Times.Once);
    }
}
