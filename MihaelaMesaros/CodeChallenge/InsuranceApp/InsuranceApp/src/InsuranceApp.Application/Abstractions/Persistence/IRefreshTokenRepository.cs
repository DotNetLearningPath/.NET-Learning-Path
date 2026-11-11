using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task AddRefreshTokenAsync(UserRefreshToken refreshToken, CancellationToken cancellationToken);

    Task RevokeAllUserTokensAsync(Guid userId, DateTime revokedAt, CancellationToken cancellationToken);
    Task<bool> RevokeRefreshTokenAsync(string tokenHash,DateTime revokedAt, CancellationToken cancellationToken);

    Task<bool> RotateRefreshTokenAsync(string oldTokenHash,UserRefreshToken newRefreshToken, DateTime revokedAt, CancellationToken cancellationToken);
}
