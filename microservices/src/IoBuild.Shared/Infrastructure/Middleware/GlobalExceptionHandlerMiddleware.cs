using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IoBuild.Shared.Infrastructure.Middleware;

/// <summary>
/// Global exception handler middleware (Decorator pattern over the pipeline).
/// Catches unhandled exceptions and returns standardized JSON error responses.
/// Maps exception types to appropriate HTTP status codes.
/// </summary>
public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    private static HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,              // 401
        InvalidOperationException   => HttpStatusCode.Conflict,                  // 409
        KeyNotFoundException        => HttpStatusCode.NotFound,                  // 404
        ArgumentException           => HttpStatusCode.BadRequest,                // 400
        _                           => HttpStatusCode.InternalServerError         // 500
    };

    private static string GetErrorMessage(Exception ex) => ex switch
    {
        UnauthorizedAccessException => "Unauthorized.",
        _                           => "An error occurred while processing your request."
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var statusCode = GetStatusCode(ex);
            logger.LogError(ex, "Unhandled exception ({StatusCode}): {Message}", (int)statusCode, ex.Message);
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                error = GetErrorMessage(ex),
                detail = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
