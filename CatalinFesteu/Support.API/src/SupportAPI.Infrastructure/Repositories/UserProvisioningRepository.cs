using Microsoft.EntityFrameworkCore;
using Npgsql;
using SupportAPI.Application.Abstractions;
using SupportAPI.Domain.Entities;
using SupportAPI.Infrastructure.Persistence;

namespace SupportAPI.Infrastructure.Repositories
{
    public class UserProvisioningRepository(SupportDbContext dbContext) : IUserProvisioningRepository
    {
        public async Task<Guid> FindOrCreateUserAsync(string name, string issuer, string subject, CancellationToken cancellationToken)
        {
            var userId = await FindUserIdAsync(issuer, subject, cancellationToken);

            if (userId != Guid.Empty) return userId;

            var user = User.Create(name, issuer, subject);

            try
            {
                await dbContext.Users.AddAsync(user, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                return user.Id;
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_Users_IdentityIssuer_IdentitySubject"
            })
            {
                dbContext.Entry(user).State = EntityState.Detached;
                var existingUserId = await FindUserIdAsync(issuer, subject, cancellationToken);

                return existingUserId;
            }
        }

        private async Task<Guid> FindUserIdAsync(string issuer, string subject, CancellationToken cancellationToken)
        {
            return await dbContext.Users
                .AsNoTracking()
                .Where(x => x.IdentityIssuer == issuer && x.IdentitySubject == subject)
                .Select(x => x.Id)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
