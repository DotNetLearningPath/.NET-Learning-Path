using Microsoft.EntityFrameworkCore;
using Npgsql;
using SupportAPI.Application.Abstractions;
using SupportAPI.Domain.Entities;
using SupportAPI.Infrastructure.Persistence;

namespace SupportAPI.Infrastructure.Repositories;

public sealed class UserProvisioningRepository(SupportDbContext dbContext) : IUserProvisioningRepository
{
    public async Task<Guid?> FindUserIdAsync(string issuer, string subject, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.IdentityIssuer == issuer && user.IdentitySubject == subject)
            .Select(user => (Guid?)user.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TryAddUserAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "UX_Users_IdentityIssuer_IdentitySubject"
        })
        {
            dbContext.Entry(user).State = EntityState.Detached;
            return false;
        }
    }
}
