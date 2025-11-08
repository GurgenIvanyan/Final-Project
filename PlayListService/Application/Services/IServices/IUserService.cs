// Application/Services/IServices/IUserService.cs
using Application.DTOs;

namespace Application.Services.IServices;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(int id, UserUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
}
