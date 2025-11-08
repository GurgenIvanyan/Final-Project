using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace User.Infrastructure.Http;

public sealed class ForwardAuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    public ForwardAuthHeaderHandler(IHttpContextAccessor http) => _http = http;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var auth = _http.HttpContext?.Request?.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(auth) &&
            auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(auth);
        }
        return base.SendAsync(request, ct);
    }
}
