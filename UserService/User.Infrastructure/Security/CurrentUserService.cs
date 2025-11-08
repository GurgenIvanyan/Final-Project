// User.Infrastructure/Security/CurrentUserService.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public ClaimsPrincipal? Principal => _http.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int? UserId
    {
        get
        {
            var sub = Principal?.FindFirst(ClaimTypes.NameIdentifier)
                      ?? Principal?.FindFirst("sub");
            return int.TryParse(sub?.Value, out var id) ? id : null;
        }
    }

    public int UserIdOrThrow()
    {
        if (!IsAuthenticated) throw new UnauthorizedAccessException("Not authenticated.");
        var id = UserId;
        if (id is null) throw new InvalidOperationException("User id claim is missing.");
        return id.Value;
    }

    public string? UserName =>
        Principal?.FindFirst(ClaimTypes.Name)?.Value
        ?? Principal?.Identity?.Name;

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();
}
