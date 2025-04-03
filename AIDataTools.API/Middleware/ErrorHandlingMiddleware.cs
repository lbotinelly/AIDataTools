using System.Net;
using System.Text.Json;

namespace AIDataTools.API.Middleware;

/// <summary>
/// Middleware for handling exceptions and returning appropriate error responses
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Process the request
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            TimeoutException => HttpStatusCode.RequestTimeout,
            _ => HttpStatusCode.InternalServerError
        };
        
        context.Response.StatusCode = (int)statusCode;
        
        var response = new
        {
            status = (int)statusCode,
            error = statusCode.ToString(),
            message = exception.Message,
            timestamp = DateTime.UtcNow
        };
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
