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

public class CartControllerTests
{
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly CartController _controller;
    private readonly string _userId = Guid.NewGuid().ToString();

    public CartControllerTests()
    {
        _cartServiceMock = new Mock<ICartService>();
        _controller = new CartController(_cartServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task AddToCart_ShouldReturnOk_WhenSuccessful()
    {
        var request = new CartRequest
        {
            ProductId = Guid.NewGuid(),
            Count = 1
        };

        _cartServiceMock.Setup(x => x.UpdateItemsAsync(_userId, request))
            .ReturnsAsync(ServerResponse.Ok());

        var result = await _controller.AddToCart(request);
        var ok = result.Result as OkObjectResult;

        ok.Should().NotBeNull();
        ((ServerResponse)ok!.Value!).Success.Should().BeTrue();
    }

    [Fact]
    public async Task AddToCart_ShouldReturnBadRequest_WhenFailed()
    {
        var request = new CartRequest
        {
            ProductId = Guid.NewGuid(),
            Count = 1
        };

        var fail = ServerResponse.Fail("error");

        _cartServiceMock.Setup(x => x.UpdateItemsAsync(_userId, request))
            .ReturnsAsync(fail);

        var result = await _controller.AddToCart(request);
        var bad = result.Result as BadRequestObjectResult;

        bad.Should().NotBeNull();
        bad!.Value.Should().Be(fail);
    }

    [Fact]
    public async Task GetCart_ShouldReturnOk_WithCartItems()
    {
        var items = new List<CartProduct>
        {
            new CartProduct { ProductId = Guid.NewGuid(), Name = "Item 1", Count = 1, Price = 10 },
            new CartProduct { ProductId = Guid.NewGuid(), Name = "Item 2", Count = 2, Price = 20 }
        };

        _cartServiceMock.Setup(x => x.GetCartAsync(_userId))
            .ReturnsAsync(ServerResponse<List<CartProduct>>.Ok(items));

        var result = await _controller.GetCart();
        var ok = result.Result as OkObjectResult;

        ok.Should().NotBeNull();
        var response = ok!.Value as ServerResponse<List<CartProduct>>;
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task GetCart_ShouldReturnBadRequest_WhenFails()
    {
        var fail = ServerResponse<List<CartProduct>>.Fail("error");

        _cartServiceMock.Setup(x => x.GetCartAsync(_userId))
            .ReturnsAsync(fail);

        var result = await _controller.GetCart();
        var bad = result.Result as BadRequestObjectResult;

        bad.Should().NotBeNull();
        bad!.Value.Should().Be(fail);
    }

    [Fact]
    public async Task BuyCart_ShouldReturnOk_WhenSuccessful()
    {
        _cartServiceMock.Setup(x => x.BuyCartAsync(_userId))
            .ReturnsAsync(ServerResponse.Ok());

        var result = await _controller.BuyCart();
        var ok = result.Result as OkObjectResult;

        ok.Should().NotBeNull();
        ((ServerResponse)ok!.Value!).Success.Should().BeTrue();
    }

    [Fact]
    public async Task BuyCart_ShouldReturnBadRequest_WhenFails()
    {
        var fail = ServerResponse.Fail("Cart is empty");

        _cartServiceMock.Setup(x => x.BuyCartAsync(_userId))
            .ReturnsAsync(fail);

        var result = await _controller.BuyCart();
        var bad = result.Result as BadRequestObjectResult;

        bad.Should().NotBeNull();
        bad!.Value.Should().Be(fail);
    }

    [Fact]
    public async Task AddToCart_ShouldReturnUnauthorized_WhenNoUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.AddToCart(new CartRequest());
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetCart_ShouldReturnUnauthorized_WhenNoUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.GetCart();
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task BuyCart_ShouldReturnUnauthorized_WhenNoUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.BuyCart();
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
}
