using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PRS.Model.Exceptions;
using PRS.Server.Middleware;
using System.Net;

public class ExceptionHandlerTests
{
    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    [Fact]
    public async Task Invoke_ShouldHandle_PrsException_AsBadRequest()
    {
        var mockLogger = new Mock<ILogger<ExceptionHandler>>();
        var context = CreateHttpContext();
        var middleware = new ExceptionHandler(
            _ => throw new PrsException("Invalid request"),
            mockLogger.Object);

        await middleware.Invoke(context);
        var body = await GetResponseBody(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        body.Should().Contain("Invalid request");
        body.Should().Contain("\"success\":false");
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Known application error occurred")),
                It.IsAny<PrsException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldHandle_GenericException_AsInternalServerError()
    {
        var mockLogger = new Mock<ILogger<ExceptionHandler>>();
        var context = CreateHttpContext();
        var middleware = new ExceptionHandler(
            _ => throw new Exception("Unexpected failure"),
            mockLogger.Object);

        await middleware.Invoke(context);
        var body = await GetResponseBody(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        body.Should().Contain("Internal server error");
        body.Should().Contain("\"success\":false");
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Unexpected server error occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
