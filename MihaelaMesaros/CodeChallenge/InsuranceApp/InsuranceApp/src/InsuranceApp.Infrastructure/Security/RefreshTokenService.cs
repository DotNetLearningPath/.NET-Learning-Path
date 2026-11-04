using InsuranceApp.Application.Abstractions.Security;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace InsuranceApp.Infrastructure.Security;

internal sealed class RefreshTokenService(IOptions<JwtSettings> options) : IRefreshTokenService
{
    public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        var expiresAt = DateTime.UtcNow.AddDays(options.Value.RefreshTokenExpirationDays);

        var token = Convert.ToBase64String(bytes);

        return (token, expiresAt);
    }

    public string HashRefreshToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}
