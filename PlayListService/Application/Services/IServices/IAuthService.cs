// Application/Services/IServices/IAuthService.cs
using Application.DTOs;

namespace Application.Services.IServices;

public interface IAuthService
{
    Task<AuthTokenDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
}
