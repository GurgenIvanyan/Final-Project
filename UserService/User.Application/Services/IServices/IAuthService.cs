using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Application.DTOs;

namespace User.Application.Services.IServices
{
    public interface IAuthService
    {
        Task<AuthTokenDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
        Task<UserDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    }
}
