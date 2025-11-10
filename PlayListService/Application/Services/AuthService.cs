
using Application.DTOs;
using Application.Services.IServices;
using Playlist.Api.Core.Interfaces.Repositories;
using Playlist.Api.Infrastructure.Security;

namespace Playlist.Api.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly JwtTokenService _jwt;

    public AuthService(IUserRepository users, JwtTokenService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<AuthTokenDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
       
         var user = await _users.GetByUserNameAsync(dto.UserName, ct);
        if (user is null) throw new UnauthorizedAccessException("Invalid credentials.");

        var hashed = PasswordHasher.Hash(dto.Password);
        if (!string.Equals(hashed, user.PasswordHash, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwt.Create(user);
        return new AuthTokenDto(token);
    }
}
