using FluentAssertions;
using Moq;
using PRS.Model.Entities;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Server.Repositories.Interfaces;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _cartRepositoryMock = new Mock<ICartRepository>();
        _cartService = new CartService(_cartRepositoryMock.Object);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnCartItems()
    {
        var userId = "user-123";
        var items = new List<CartProduct>
        {
            new CartProduct
            {
                ProductId = Guid.NewGuid(),
                Name = "Test Product",
                Price = 9.99m,
                Count = 2,
                Image = null
            }
        };

        _cartRepositoryMock.Setup(r => r.GetCartItemsAsync(userId))
                           .ReturnsAsync(items);

        var result = await _cartService.GetCartAsync(userId);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task UpdateItemsAsync_ShouldAddOrUpdate_WhenCountIsGreaterThanZero()
    {
        var userId = "user-123";
        var request = new CartRequest
        {
            ProductId = Guid.NewGuid(),
            Count = 3
        };

        var result = await _cartService.UpdateItemsAsync(userId, request);

        result.Success.Should().BeTrue();

        _cartRepositoryMock.Verify(r =>
            r.AddOrUpdateCartItemAsync(userId, request.ProductId, request.Count), Times.Once);

        _cartRepositoryMock.Verify(r =>
            r.RemoveCartItemAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateItemsAsync_ShouldRemove_WhenCountIsZero()
    {
        var userId = "user-123";
        var request = new CartRequest
        {
            ProductId = Guid.NewGuid(),
            Count = 0
        };

        var result = await _cartService.UpdateItemsAsync(userId, request);

        result.Success.Should().BeTrue();

        _cartRepositoryMock.Verify(r =>
            r.RemoveCartItemAsync(userId, request.ProductId), Times.Once);

        _cartRepositoryMock.Verify(r =>
            r.AddOrUpdateCartItemAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task BuyCartAsync_ShouldThrow_WhenCartIsEmpty()
    {
        var userId = "user-123";
        _cartRepositoryMock.Setup(r => r.GetCartItemsAsync(userId))
                           .ReturnsAsync(new List<CartProduct>());

        Func<Task> act = async () => await _cartService.BuyCartAsync(userId);

        await act.Should().ThrowAsync<CartIsEmptyException>();
    }

    [Fact]
    public async Task BuyCartAsync_ShouldBuy_WhenCartHasItems()
    {
        var userId = "user-123";
        var items = new List<CartProduct>
        {
            new CartProduct
            {
                ProductId = Guid.NewGuid(),
                Name = "Item 1",
                Price = 10,
                Count = 1
            },
            new CartProduct
            {
                ProductId = Guid.NewGuid(),
                Name = "Item 2",
                Price = 20,
                Count = 2
            }
        };

        _cartRepositoryMock.Setup(r => r.GetCartItemsAsync(userId))
                           .ReturnsAsync(items);

        var result = await _cartService.BuyCartAsync(userId);

        result.Success.Should().BeTrue();

        var expectedIds = items.Select(i => i.ProductId).ToList();

        _cartRepositoryMock.Verify(r =>
            r.BuyCartAsync(userId, It.Is<List<Guid>>(ids =>
                ids.SequenceEqual(expectedIds))), Times.Once);
    }
}
