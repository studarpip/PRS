using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PRS.Model.Entities;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Controllers;
using PRS.Server.Services.Interfaces;

public class AdminControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _controller = new AdminController(_serviceMock.Object);
    }

    [Fact]
    public async Task Search_ShouldReturnOk_WhenSuccessful()
    {
        var request = new ProductSearchRequest { Page = 1, PageSize = 10 };
        var response = ServerResponse<List<Product>>.Ok(new List<Product>());

        _serviceMock.Setup(s => s.SearchAsync(request, null))
                    .ReturnsAsync(response);

        var result = await _controller.Search(request);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenSuccessful()
    {
        var request = new ProductCreateEditRequest
        {
            Name = "Item",
            Price = 20,
            Categories = new List<PRS.Model.Enums.Category> { PRS.Model.Enums.Category.Gaming }
        };

        var response = ServerResponse.Ok();
        _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);

        var result = await _controller.Create(request);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be(response);
    }

    [Fact]
    public async Task Edit_ShouldReturnOk_WhenSuccessful()
    {
        var id = Guid.NewGuid();
        var request = new ProductCreateEditRequest
        {
            Name = "Updated",
            Price = 30,
            Categories = new List<PRS.Model.Enums.Category> { PRS.Model.Enums.Category.Gaming }
        };

        var response = ServerResponse.Ok();
        _serviceMock.Setup(s => s.UpdateAsync(id, request)).ReturnsAsync(response);

        var result = await _controller.Edit(id, request);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be(response);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenSuccessful()
    {
        var id = Guid.NewGuid();
        var response = ServerResponse.Ok();

        _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(response);

        var result = await _controller.Delete(id);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be(response);
    }

    [Fact]
    public async Task Search_ShouldReturnBadRequest_WhenFailed()
    {
        var request = new ProductSearchRequest { Page = 1, PageSize = 10 };
        var response = ServerResponse<List<Product>>.Fail("fail");

        _serviceMock.Setup(s => s.SearchAsync(request, null)).ReturnsAsync(response);

        var result = await _controller.Search(request);

        var bad = result.Result as BadRequestObjectResult;
        bad.Should().NotBeNull();
        bad!.Value.Should().Be(response);
    }
}
