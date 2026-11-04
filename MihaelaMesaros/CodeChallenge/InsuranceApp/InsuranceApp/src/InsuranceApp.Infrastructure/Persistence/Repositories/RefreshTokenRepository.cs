using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(InsuranceDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<UserRefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return await dbContext.UserRefreshTokens
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(UserRefreshToken refreshToken, CancellationToken cancellationToken)
    {
        dbContext.UserRefreshTokens.Add(refreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, DateTime revokedAt, CancellationToken cancellationToken)
    {
        await dbContext.UserRefreshTokens
            .Where(x =>x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > revokedAt)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.RevokedAt, revokedAt), cancellationToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string tokenHash, DateTime revokedAt, CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.UserRefreshTokens
            .Where(x => x.TokenHash == tokenHash && x.RevokedAt == null && x.ExpiresAt > revokedAt)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.RevokedAt, revokedAt),
                cancellationToken);

        return affectedRows == 1;
    }

    public async Task<bool> RotateRefreshTokenAsync(string oldTokenHash, UserRefreshToken newRefreshToken, DateTime revokedAt, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var affectedRows = await dbContext.UserRefreshTokens
            .Where(x =>
                x.TokenHash == oldTokenHash
                && x.UserId == newRefreshToken.UserId 
                && x.User.IsActive
                && x.ExpiresAt > revokedAt
                && x.RevokedAt == null
            ).ExecuteUpdateAsync(
                x => x.SetProperty(x => x.RevokedAt, revokedAt),
                cancellationToken);

        if (affectedRows != 1)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        dbContext.UserRefreshTokens.Add(newRefreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return true;
    }
}