using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Application.DTOs.Auth;
using InsuranceApp.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace InsuranceApp.Infrastructure.Security;

internal sealed class JwtService(IOptions<JwtSettings> options) : IJwtService
{
    public AccessTokenDto GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var settings = options.Value;

        var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(settings.SecretKey));

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.RoleId.ToString())
        };

        if (user.BrokerId.HasValue)
        {
            claims.Add(new Claim("brokerId", user.BrokerId.Value.ToString()));
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(settings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: expiresAt,
            notBefore: now,
            signingCredentials: credentials
        );

        return new AccessTokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}