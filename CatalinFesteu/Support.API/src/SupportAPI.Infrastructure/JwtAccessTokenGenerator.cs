using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Results;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SupportAPI.Infrastructure
{
    public class JwtAccessTokenGenerator(IOptions<JwtSettings> options) : IAccessTokenGenerator
    {
        public IssuedAccessToken Create(Guid userId, string name)
        {
            var settings = options.Value;

            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(settings.AccessTokenMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, name)
            };
            
            var keyBytes = Convert.FromBase64String(settings.SigningKey);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);
                
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new IssuedAccessToken(accessToken, expiresAt);
        }
    }
}
