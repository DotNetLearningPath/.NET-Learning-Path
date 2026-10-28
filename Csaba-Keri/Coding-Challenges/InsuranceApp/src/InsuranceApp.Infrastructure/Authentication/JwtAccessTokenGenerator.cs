using InsuranceApp.Application.Authentication;
using InsuranceApp.Application.Authentication.Results;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InsuranceApp.Infrastructure.Authentication;

public class JwtAccessTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider) : IAccessTokenGenerator
{
    private readonly JwtOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    public AccessTokenResult Generate(AuthenticatedAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        var now = _timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.AccessTokenLifetimeMinutes);
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.UserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtClaimNames.Role, account.Role)
        };

        if (account.BrokerId is Guid brokerId)
        {
            claims.Add(new(JwtClaimNames.BrokerId, brokerId.ToString()));
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
                SecurityAlgorithms.HmacSha256
            )
        });

        return new(
            accessToken: tokenHandler.WriteToken(token),
            expiresAt: expiresAt
        );
    }
}
