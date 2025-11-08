using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Playlist.Api.Core.Entities;


namespace Playlist.Api.Infrastructure.Security;


public class JwtTokenService
{
    private readonly JwtOptions _opt;
    public JwtTokenService(JwtOptions opt) => _opt = opt;


    public string Create(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
new Claim(ClaimTypes.Name, user.UserName),
new Claim(ClaimTypes.Role, user.Role)
};
        var token = new JwtSecurityToken(_opt.Issuer, _opt.Audience, claims, expires: DateTime.UtcNow.AddMinutes(_opt.ExpMinutes), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}