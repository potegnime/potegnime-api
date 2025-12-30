using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using PotegniMe.Core.Exceptions;

namespace PotegniMe.Core;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode status;
        ErrorResponseDto response;

        switch (exception)
        {
            case NotFoundException:
                status = HttpStatusCode.NotFound;
                response = new ErrorResponseDto { ErrorCode = 1, Message = string.Empty };
                break;
            case ConflictException e:
                status = HttpStatusCode.Conflict;
                response = new ErrorResponseDto { ErrorCode = 1, Message = e.Message };
                break;
            case ArgumentException e:
                status = HttpStatusCode.BadRequest;
                response = new ErrorResponseDto { ErrorCode = 1, Message = e.Message };
                break;
            case UnauthorizedAccessException e:
                status = HttpStatusCode.Forbidden;
                response = new ErrorResponseDto { ErrorCode = 1, Message = e.Message };
                break;
            default:
                status = HttpStatusCode.InternalServerError;
                response = new ErrorResponseDto { ErrorCode = 2, Message = "Internal server error." };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}