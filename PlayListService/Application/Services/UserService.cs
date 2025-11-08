// Application/Services/UserService.cs
using Application.DTOs;
using Application.Services.IServices;
using Playlist.Api.Core.Entities;
using Playlist.Api.Core.Interfaces.Repositories;
using Playlist.Api.Infrastructure.Security;

// 👇 алиас, чтобы отличать доменную сущность от корневого namespace "User"
using CoreUser = Playlist.Api.Core.Entities.User;

namespace Playlist.Api.Application.Services;

public class UserService : IUserService
{
    private static readonly HashSet<string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase) { "Admin", "User" };

    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public UserService(IUserRepository users, IUnitOfWork uow)
    {
        _users = users;
        _uow = uow;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName))
            throw new ArgumentException("UserName is required.", nameof(dto.UserName));
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password is required.", nameof(dto.Password));

        var userName = dto.UserName.Trim();

        if (await _users.ExistsByUserNameAsync(userName, ct))
            throw new InvalidOperationException("UserName already exists.");

        var entity = new CoreUser
        {
            UserName = userName,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Role = "User"
        };

        await _uow.ExecuteInTransactionAsync(async _ =>
        {
            await _users.AddAsync(entity, ct);
        }, ct);

        return new UserDto(entity.Id, entity.UserName, entity.Role);
    }

    public async Task<UserDto> UpdateAsync(int id, UserUpdateDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct)
                   ?? throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(dto.UserName))
        {
            var newName = dto.UserName.Trim();
            if (!newName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase) &&
                await _users.ExistsByUserNameExceptAsync(newName, id, ct))
                throw new InvalidOperationException("UserName already exists.");

            user.UserName = newName;
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            user.Role = NormalizeRole(dto.Role);
        }

        await _uow.ExecuteInTransactionAsync(async _ =>
        {
            await _users.UpdateAsync(user, ct);
        }, ct);

        return new UserDto(user.Id, user.UserName, user.Role);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct)
                   ?? throw new KeyNotFoundException("User not found.");

        await _uow.ExecuteInTransactionAsync(async _ =>
        {
            await _users.DeleteAsync(user, ct);
        }, ct);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _users.GetAllAsync(ct);
        return items
            .OrderBy(u => u.UserName)
            .Select(u => new UserDto(u.Id, u.UserName, u.Role))
            .ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var u = await _users.GetByIdAsync(id, ct);
        return u is null ? null : new UserDto(u.Id, u.UserName, u.Role);
    }

    private static string NormalizeRole(string? input)
    {
        var role = string.IsNullOrWhiteSpace(input) ? "User" : input.Trim();
        if (!AllowedRoles.Contains(role)) role = "User";
        return role;
    }
}
