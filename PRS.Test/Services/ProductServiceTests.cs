using FluentAssertions;
using Moq;
using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _productService = new ProductService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        var userId = "user-123";
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test",
            Price = 10m,
            Categories = new List<Category>()
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, userId))
                       .ReturnsAsync(product);

        var result = await _productService.GetByIdAsync(productId, userId);

        result.Success.Should().BeTrue();
        result.Data.Should().Be(product);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
    {
        var productId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<string>()))
                       .ReturnsAsync((Product?)null);

        Func<Task> act = async () => await _productService.GetByIdAsync(productId, "user-123");

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnList()
    {
        var request = new ProductSearchRequest
        {
            Page = 1,
            PageSize = 10
        };

        var products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "P1", Price = 5, Categories = new List<Category>() }
        };

        _repositoryMock.Setup(r => r.SearchAsync(request, null))
                       .ReturnsAsync(products);

        var result = await _productService.SearchAsync(request, null);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenPriceInvalid()
    {
        var request = new ProductCreateEditRequest
        {
            Name = "Test",
            Price = 0
        };

        Func<Task> act = async () => await _productService.CreateAsync(request);

        await act.Should().ThrowAsync<PriceException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepository_WhenValid()
    {
        var request = new ProductCreateEditRequest
        {
            Name = "Test",
            Price = 10,
            Categories = new List<Category>()
        };

        var result = await _productService.CreateAsync(request);

        result.Success.Should().BeTrue();

        _repositoryMock.Verify(r => r.CreateAsync(It.Is<Product>(p =>
            p.Name == request.Name && p.Price == request.Price)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenPriceInvalid()
    {
        var request = new ProductCreateEditRequest
        {
            Name = "Test",
            Price = -5
        };

        Func<Task> act = async () => await _productService.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<PriceException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepository_WhenValid()
    {
        var request = new ProductCreateEditRequest
        {
            Name = "Updated",
            Price = 99,
            Categories = new List<Category>()
        };

        var id = Guid.NewGuid();

        var result = await _productService.UpdateAsync(id, request);

        result.Success.Should().BeTrue();

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p =>
            p.Id == id && p.Name == request.Name && p.Price == request.Price)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();

        var result = await _productService.DeleteAsync(id);

        result.Success.Should().BeTrue();

        _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
