// Web/Controllers/AuthController.cs
using Application.DTOs;
using Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Playlist.Api.Web.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IUserService _users;

        public AuthController(IAuthService auth, IUserService users)
        {
            _auth = auth;
            _users = users;
        }

        /// <summary>Register new user (public)</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public Task<UserDto> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
            => _users.RegisterAsync(dto, ct);

        /// <summary>Login and get JWT (public)</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status200OK)]
        public Task<AuthTokenDto> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
            => _auth.LoginAsync(dto, ct);

        /// <summary>Get all users (admin only)</summary>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
        public Task<IReadOnlyList<UserDto>> GetAllUsers(CancellationToken ct)
            => _users.GetAllAsync(ct);

        /// <summary>Update user (admin only)</summary>
        [HttpPut("users/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<UserDto> UpdateUser(int id, [FromBody] UserUpdateDto dto, CancellationToken ct)
            => _users.UpdateAsync(id, dto, ct);

        /// <summary>Delete user (admin only)</summary>
        [HttpDelete("users/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
        {
            await _users.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
