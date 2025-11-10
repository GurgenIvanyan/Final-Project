
using System.Security.Claims;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    int UserIdOrThrow();                
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    ClaimsPrincipal? Principal { get; }
}
