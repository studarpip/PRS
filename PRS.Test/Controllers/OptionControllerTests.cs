using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PRS.Model.Entities;
using PRS.Model.Responses;
using PRS.Server.Controllers.PRS.Server.Controllers;

public class OptionControllerTests
{
    private readonly OptionController _controller;

    public OptionControllerTests()
    {
        _controller = new OptionController();
    }

    [Fact]
    public void GetOrderBy_ShouldReturnAllOrderByOptions()
    {
        var result = _controller.GetOrderBy();

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();

        var response = ok!.Value as ServerResponse<List<EnumOption>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNullOrEmpty();
        response.Data.Should().OnlyContain(opt => opt.Label != null);
    }

    [Fact]
    public void GetCategories_ShouldReturnAllCategoryOptions()
    {
        var result = _controller.GetCategories();

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();

        var response = ok!.Value as ServerResponse<List<EnumOption>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNullOrEmpty();
        response.Data.Should().OnlyContain(opt => opt.Label != null);
    }

    [Fact]
    public void GetRegistrationOptions_ShouldReturnGenderAndCountryOptions()
    {
        var result = _controller.GetRegistrationOptions();

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();

        var response = ok!.Value as ServerResponse<RegistrationOptionsResponse>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.Genders.Should().NotBeNullOrEmpty();
        response.Data.Countries.Should().NotBeNullOrEmpty();
    }
}
