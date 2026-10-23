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
            try
            {
                await SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgresException)
            {
                //TO Extend
                throw new PersistenceException(postgresException.Message);
            }
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
             await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

