using System.Net;
using System.Text.Json;
using Application.Common.Errors;

namespace User.Infrastructure.Http;

public sealed class ProblemDetailsHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var resp = await base.SendAsync(req, ct);
        if (resp.IsSuccessStatusCode) return resp;

        var status = (HttpStatusCode)resp.StatusCode;
        string body = await resp.Content.ReadAsStringAsync(ct);
        string message = $"Downstream error ({(int)status})";

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("title", out var t)) message = t.GetString() ?? message;
            var type = root.TryGetProperty("type", out var tp) ? tp.GetString() : null;

            switch (status)
            {
                case HttpStatusCode.Unauthorized: throw new UnauthorizedAccessException(message);
                case HttpStatusCode.Forbidden: throw new ForbiddenException(message);
                case HttpStatusCode.NotFound: throw new NotFoundException(message);
                case HttpStatusCode.Conflict: throw new ConflictException(message);
                case (HttpStatusCode)422:
                    var errors = new Dictionary<string, string[]>();
                    if (root.TryGetProperty("errors", out var e) && e.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in e.EnumerateObject())
                            errors[prop.Name] = prop.Value.EnumerateArray().Select(v => v.GetString() ?? "Invalid").ToArray();
                    }
                    throw new ValidationException(message, errors);
                default:
                    throw new InvalidOperationException(message);
            }
        }
        catch (DomainException) { throw; }
        catch (UnauthorizedAccessException) { throw; }
        catch
        {
            if (status == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException(message);
            throw new InvalidOperationException(message);
        }
    }
}
