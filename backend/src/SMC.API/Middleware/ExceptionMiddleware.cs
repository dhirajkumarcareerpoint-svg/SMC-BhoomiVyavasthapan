using System.Net;
using System.Text.Json;
using SMC.Application.Common;

namespace SMC.API.Middleware;

/// <summary>सर्व unhandled exceptions पकडून एकसंध मराठी JSON प्रतिसाद देते.</summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(JsonSerializer.Serialize(ApiResponse<object>.Fail(ex.Message)));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync(JsonSerializer.Serialize(ApiResponse<object>.Fail("या कृतीसाठी आपल्याला अधिकार नाहीत.")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(JsonSerializer.Serialize(
                ApiResponse<object>.Fail("अनपेक्षित त्रुटी आली आहे. कृपया पुन्हा प्रयत्न करा किंवा प्रशासकाशी संपर्क साधा.")));
        }
    }
}
