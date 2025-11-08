using System.Net;
using System.Text.Json;
using User.Application.Common.Errors; // AppErrors + наши DomainException-типы
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Application.Common.Errors;

namespace User.Web.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log)
    { _next = next; _log = log; }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (UnauthorizedAccessException)
        {
            await Write(ctx, HttpStatusCode.Unauthorized, AppErrors.Unauthorized, AppErrors.UserUnauthorized());
        }
        catch (ForbiddenException ex)
        {
            await Write(ctx, HttpStatusCode.Forbidden, AppErrors.Forbidden, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await Write(ctx, HttpStatusCode.NotFound, AppErrors.NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            await Write(ctx, HttpStatusCode.Conflict, AppErrors.Conflict, ex.Message);
        }
        catch (ValidationException ex)
        {
            await WriteValidation(ctx, AppErrors.Validation, AppErrors.ValidationFailed(), ex.Errors);
        }
        catch (InvalidOperationException ex)
        {
            await Write(ctx, HttpStatusCode.BadRequest, AppErrors.BadRequest, ex.Message);
        }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            // Nginx style 499
            await Write(ctx, (HttpStatusCode)499, AppErrors.BadRequest, "Client closed the request.");
        }
        catch (Exception ex)
        {
            var traceId = ctx.TraceIdentifier;
            _log.LogError(ex, "Unhandled at {Path} traceId={TraceId}", ctx.Request.Path, traceId);
            await Write(ctx, HttpStatusCode.InternalServerError, AppErrors.Unexpected, "Unexpected server error.");
        }
    }

    private static Task Write(HttpContext ctx, HttpStatusCode code, string type, string title)
    {
        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = (int)code;

        var payload = new
        {
            type,
            title,
            status = (int)code,
            instance = ctx.Request.Path.ToString(),
            traceId = ctx.TraceIdentifier
        };

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOpts));
    }

    private static Task WriteValidation(
        HttpContext ctx,
        string type,
        string title,
        IDictionary<string, string[]> errors)
    {
        const int status = 422;
        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = status;

        var payload = new
        {
            type,
            title,
            status,
            instance = ctx.Request.Path.ToString(),
            traceId = ctx.TraceIdentifier,
            errors
        };

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOpts));
    }
}
