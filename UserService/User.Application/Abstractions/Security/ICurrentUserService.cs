// User.Application/Abstractions/Security/ICurrentUserService.cs
using System.Security.Claims;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    int UserIdOrThrow();                 // бросит 401/InvalidOperation если нет
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    ClaimsPrincipal? Principal { get; }
}
