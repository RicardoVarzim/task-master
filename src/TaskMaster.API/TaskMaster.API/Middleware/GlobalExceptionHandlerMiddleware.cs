using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TaskMaster.API.Middleware;

/// <summary>
/// Middleware for centralized error handling
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request.";

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case KeyNotFoundException:
            case FileNotFoundException:
                code = HttpStatusCode.NotFound;
                message = exception.Message;
                break;

            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                message = "You are not authorized to perform this action.";
                break;

            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case DbUpdateException dbEx:
                code = HttpStatusCode.BadRequest;
                message = "A database error occurred. Please check your input.";
                // Log the actual database error for debugging
                break;
        }

        var result = JsonSerializer.Serialize(new ErrorResponse
        {
            StatusCode = (int)code,
            Message = message,
            Details = exception.Message,
            Timestamp = DateTime.UtcNow
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}

