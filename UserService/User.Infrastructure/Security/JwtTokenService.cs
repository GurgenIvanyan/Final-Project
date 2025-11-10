using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using User.Application.Abstractions.Security;  

namespace User.Infrastructure.Security
{
    public class JwtTokenService : IJwtTokenService   
    {
        private readonly JwtOptions _opt;
        public JwtTokenService(JwtOptions opt) => _opt = opt;

        public string Create(int userId, string userName, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_opt.ExpMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
