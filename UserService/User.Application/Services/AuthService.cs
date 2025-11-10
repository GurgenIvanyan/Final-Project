
using User.Application.DTOs;
using User.Application.Services.IServices;
using User.Core.Interfaces.Repositories;
using User.Application.Abstractions.Security;   

using UserEntity = User.Core.Entities.User;

namespace User.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenService _jwt;
        private readonly IPasswordHasher _hasher;

        public AuthService(
            IUserRepository users,
            IUnitOfWork uow,
            IJwtTokenService jwt,
            IPasswordHasher hasher)
        {
            _users = users; _uow = uow; _jwt = jwt; _hasher = hasher;
        }

        public async Task<UserDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName)) throw new ArgumentException("UserName is required");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required");

            var userName = dto.UserName.Trim();
            if (await _users.ExistsByUserNameAsync(userName, ct))
                throw new InvalidOperationException("UserName already exists.");

            var entity = new UserEntity
            {
                UserName = userName,
                PasswordHash = _hasher.Hash(dto.Password)
            };

            await _uow.ExecuteInTransactionAsync(async _ =>
            {
                await _users.AddAsync(entity, ct);
            }, ct);

            return new UserDto(entity.Id, entity.UserName, entity.Role);
        }

        public async Task<AuthTokenDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
        {
            var user = await _users.GetByUserNameAsync(dto.UserName, ct)
                       ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!string.Equals(_hasher.Hash(dto.Password), user.PasswordHash, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var token = _jwt.Create(user.Id, user.UserName, user.Role);
            return new AuthTokenDto(token);
        }
    }
}
