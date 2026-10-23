using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using InsuranceApp.Application.DTOs.Auth;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Application.Abstractions.Security;

namespace InsuranceApp.Infrastructure.Security;

internal sealed class JwtService(IConfiguration configuration) : IJwtService
{
    public LoginResponseDto GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var secretKey = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT secret key is not configured.");

        var issuer = configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");

        var audience = configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");

        if (!int.TryParse(configuration["JwtSettings:ExpirationMinutes"], out var expirationMinutes) || expirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT expiration is not configured correctly.");
        }

        var keyBytes = Convert.FromBase64String(secretKey);

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must contain at least 32 bytes.");
        }

        var signingKey = new SymmetricSecurityKey(keyBytes);

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
        var expiresAt = now.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            notBefore: now,
            signingCredentials: credentials
        );

        return new LoginResponseDto(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}