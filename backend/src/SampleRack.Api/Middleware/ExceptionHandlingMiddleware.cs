using System.Text.Json;
using SampleRack.Application.Exceptions;
using SampleRack.SharedKernel.Exceptions;

namespace SampleRack.Api.Middleware;

/// <summary>
/// Converts unhandled domain/application exceptions into consistent JSON error
/// responses with the right HTTP status code.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, payload) = ex switch
        {
            ValidationException v => (StatusCodes.Status400BadRequest,
                (object)new { error = v.Message, code = "Validation", errors = v.Errors }),
            NotFoundException nf => (StatusCodes.Status404NotFound,
                new { error = nf.Message, code = "NotFound" }),
            ConflictException c => (StatusCodes.Status409Conflict,
                new { error = c.Message, code = "Conflict" }),
            ForbiddenAccessException f => (StatusCodes.Status403Forbidden,
                new { error = f.Message, code = "Forbidden" }),
            BusinessRuleException b => (StatusCodes.Status400BadRequest,
                new { error = b.Message, code = "BusinessRule" }),
            DomainException d => (StatusCodes.Status400BadRequest,
                new { error = d.Message, code = "DomainRule" }),
            _ => (StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred.", code = "ServerError" })
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);
        else
            _logger.LogWarning("{Code} on {Path}: {Message}", payload.GetType().Name, context.Request.Path, ex.Message);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
