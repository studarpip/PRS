using PRS.Model.Exceptions;
using System.Net;
using System.Text.Json;

namespace PRS.Server.Middleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (PrsException ex)
            {
                _logger.LogWarning(ex, "Known application error occurred");
                await HandleExceptionAsync(context, ex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected server error occurred");
                await HandleExceptionAsync(context, "Internal server error", HttpStatusCode.InternalServerError);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, string message, HttpStatusCode code)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var response = new
            {
                success = false,
                errorMessage = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
