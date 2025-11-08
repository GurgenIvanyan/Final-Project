using System.Net;
using System.Text.Json;
using Application.Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Playlist.Api.Web.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log)
    { _next = next; _log = log; }

    public async Task Invoke(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (UnauthorizedAccessException ex)
        { await Write(ctx, HttpStatusCode.Unauthorized, "auth.unauthorized", ex.Message); }
        catch (ForbiddenException ex)
        { await Write(ctx, HttpStatusCode.Forbidden, "auth.forbidden", ex.Message); }
        catch (NotFoundException ex)
        { await Write(ctx, HttpStatusCode.NotFound, "common.not_found", ex.Message); }
        catch (ConflictException ex)
        { await Write(ctx, HttpStatusCode.Conflict, "common.conflict", ex.Message); }
        catch (ValidationException ex)
        { await WriteValidation(ctx, "common.validation", ex.Message, ex.Errors); }
        catch (InvalidOperationException ex)
        { await Write(ctx, HttpStatusCode.BadRequest, "common.bad_request", ex.Message); }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        { await Write(ctx, (HttpStatusCode)499, "common.client_closed", "Client closed the request."); }
        catch (Exception ex)
        {
            var traceId = ctx.TraceIdentifier;
            _log.LogError(ex, "Unhandled at {Path}, traceId={TraceId}", ctx.Request.Path, traceId);
            await Write(ctx, HttpStatusCode.InternalServerError, "common.unexpected", "Unexpected server error.");
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

    private static Task WriteValidation(HttpContext ctx, string type, string title, IDictionary<string, string[]> errors)
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
