using User.Application.Services.IServices;
using User.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace User.Web.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        [AllowAnonymous]
        public Task<UserDto> Register([FromBody] RegisterRequestDto dto, CancellationToken ct) => _auth.RegisterAsync(dto, ct);

        [HttpPost("login")]
        [AllowAnonymous]
        public Task<AuthTokenDto> Login([FromBody] LoginRequestDto dto, CancellationToken ct) => _auth.LoginAsync(dto, ct);
    }
}
