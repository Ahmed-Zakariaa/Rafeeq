using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rafeeq.Domain.Common;
using Rafeeq.Infrastructure.Localization;

namespace Rafeeq.Infrastructure.Middleware;

/// <summary>
/// Central error handling: turns BusinessException into a clean 400 + ResultViewModel, and any
/// other exception into a logged 500. Never leaks stack traces to clients.
/// </summary>
public class ExceptionMiddleware
{
    // PascalCase to match the rest of the API envelope (the FE reads Data/IsSuccess/Message).
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = null };
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (BusinessException ex)
        {
            var lang = ResolveLang(ctx);
            var errors = ex.Errors
                .Select(e => new ErrorMessageDto
                {
                    PropertyName = e.PropertyName,
                    ErrorMessage = Localizer.Translate(e.ErrorMessage, lang)
                })
                .ToList();

            await Write(ctx, HttpStatusCode.BadRequest, new ResultViewModel<object>
            {
                IsSuccess = false,
                Message = errors.FirstOrDefault()?.ErrorMessage,
                Data = errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await Write(ctx, HttpStatusCode.InternalServerError, new ResultViewModel<object>
            {
                IsSuccess = false,
                Message = Localizer.Translate("unexpectedError", ResolveLang(ctx))
            });
        }
    }

    // Same rule as ICurrentUser: "en" only when Accept-Language starts with en, else Arabic.
    private static string ResolveLang(HttpContext ctx)
    {
        var header = ctx.Request.Headers.AcceptLanguage.ToString();
        return !string.IsNullOrWhiteSpace(header)
            && header.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "ar";
    }

    private static async Task Write(HttpContext ctx, HttpStatusCode status, object body)
    {
        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOpts));
    }
}
