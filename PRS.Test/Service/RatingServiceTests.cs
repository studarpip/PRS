using FluentAssertions;
using Moq;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services;

public class RatingServiceTests
{
    private readonly Mock<IRatingRepository> _ratingRepositoryMock;
    private readonly RatingService _ratingService;

    public RatingServiceTests()
    {
        _ratingRepositoryMock = new Mock<IRatingRepository>();
        _ratingService = new RatingService(_ratingRepositoryMock.Object);
    }

    [Fact]
    public async Task CanRateAsync_ShouldReturnCheckResponse()
    {
        var userId = "user-123";
        var productId = Guid.NewGuid();
        var response = new RatingCheckResponse
        {
            CanRate = true,
            PreviousRating = 4
        };

        _ratingRepositoryMock
            .Setup(r => r.CheckIfUserCanRateAsync(userId, productId))
            .ReturnsAsync(response);

        var result = await _ratingService.CanRateAsync(userId, productId);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task SubmitRatingAsync_ShouldCallSubmit_WhenAllowed()
    {
        var userId = "user-123";
        var productId = Guid.NewGuid();
        var request = new RatingRequest
        {
            ProductId = productId,
            Rating = 5
        };

        _ratingRepositoryMock
            .Setup(r => r.CheckIfUserCanRateAsync(userId, productId))
            .ReturnsAsync(new RatingCheckResponse
            {
                CanRate = true
            });

        var result = await _ratingService.SubmitRatingAsync(userId, request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _ratingRepositoryMock.Verify(r =>
            r.SubmitRatingAsync(userId, productId, request.Rating), Times.Once);
    }

    [Fact]
    public async Task SubmitRatingAsync_ShouldThrow_WhenNotAllowed()
    {
        var userId = "user-123";
        var productId = Guid.NewGuid();
        var request = new RatingRequest
        {
            ProductId = productId,
            Rating = 2
        };

        _ratingRepositoryMock
            .Setup(r => r.CheckIfUserCanRateAsync(userId, productId))
            .ReturnsAsync(new RatingCheckResponse
            {
                CanRate = false
            });

        Func<Task> act = async () => await _ratingService.SubmitRatingAsync(userId, request);

        await act.Should().ThrowAsync<CannotRateProductException>();

        _ratingRepositoryMock.Verify(r =>
            r.SubmitRatingAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }
}
