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

public class ProductControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly ProductController _controller;
    private readonly string _userId;

    public ProductControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _controller = new ProductController(_productServiceMock.Object);

        _userId = Guid.NewGuid().ToString();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _userId) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProductFound()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", Price = 10 };

        _productServiceMock.Setup(x => x.GetByIdAsync(productId, _userId))
            .ReturnsAsync(ServerResponse<Product>.Ok(product));

        var result = await _controller.GetById(productId);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeOfType<ServerResponse<Product>>();
        ((ServerResponse<Product>)ok.Value!).Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ShouldReturnUnauthorized_WhenNoUser()
    {
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();

        var result = await _controller.GetById(Guid.NewGuid());
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Search_ShouldReturnOk_WhenValid()
    {
        var request = new ProductSearchRequest { Input = "test", Page = 1, PageSize = 10 };
        var products = new List<Product> { new Product { Name = "A", Price = 10 } };

        _productServiceMock.Setup(x => x.SearchAsync(request, _userId))
            .ReturnsAsync(ServerResponse<List<Product>>.Ok(products));

        var result = await _controller.Search(request);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var response = ok!.Value as ServerResponse<List<Product>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_ShouldReturnUnauthorized_WhenNoUser()
    {
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();

        var result = await _controller.Search(new ProductSearchRequest());
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenServiceFails()
    {
        var productId = Guid.NewGuid();
        var failResponse = ServerResponse<Product>.Fail("Not found");

        _productServiceMock.Setup(x => x.GetByIdAsync(productId, _userId))
            .ReturnsAsync(failResponse);

        var result = await _controller.GetById(productId);

        var badRequest = result.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        ((ServerResponse<Product>)badRequest!.Value!).Success.Should().BeFalse();
    }
}
