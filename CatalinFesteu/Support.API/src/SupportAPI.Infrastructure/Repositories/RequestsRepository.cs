using Microsoft.EntityFrameworkCore;
using Npgsql;
using SupportAPI.Application.Abstractions;
using SupportAPI.Domain.Entities;
using SupportAPI.Infrastructure.Exceptions;
using SupportAPI.Infrastructure.Persistence;

namespace SupportAPI.Infrastructure.Repositories
{
    public class RequestsRepository(SupportDbContext dbContext) : IRequestsRepository
    {
        public async Task AddRequestAsync(Request request, CancellationToken cancellationToken)
        {
            await dbContext.Requests.AddAsync(request, cancellationToken);
        }

        public async Task<Request?> GetRequestByIdAsync(Guid userId, Guid requestId, CancellationToken cancellationToken)
        {
            return await dbContext.Requests
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    request => request.UserId == userId &&
                               request.Id == requestId,
                    cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException)
            {
                throw new PersistenceException("Saving request changes failed.", exception);
            }
        }
    }
}
