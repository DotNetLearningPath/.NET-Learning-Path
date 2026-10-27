using Insurance.Application.Interfaces;
using Insurance.Application.Exceptions;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Insurance.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly InsuranceDbContext _context;

        public ClientRepository(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Clients
                .Include(client => client.Buildings)
                .FirstOrDefaultAsync(
                    client => client.Id == id,
                    cancellationToken);
        }

        public async Task<Client?> GetClientByIdWithBuildingsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Clients
                .AsNoTracking()
                .Include(client => client.Buildings)
                .FirstOrDefaultAsync(
                    client => client.Id == id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Client>> SearchClientsAsync(string? name, string? identificationNumber, int page, int pageSize, CancellationToken cancellationToken)
        {
            IQueryable<Client> query = _context.Clients.AsNoTracking();


            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(client => client.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(identificationNumber))
            {
                query = query.Where(client => client.IdentificationNumber == identificationNumber);
            }

            return await query.OrderBy(client => client.Name)
                .ThenBy(client => client.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsClientByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken)
        {
            return await _context.Clients
                .AnyAsync(client => client.IdentificationNumber == identificationNumber, cancellationToken);
        }

        public async Task AddAsync(Client client, CancellationToken cancellationToken)
        {
            await _context.Clients.AddAsync(client, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqliteException sqliteEx
                    && sqliteEx.SqliteExtendedErrorCode == 2067 && sqliteEx.Message.Contains(
                        "Clients.IdentificationNumber", StringComparison.OrdinalIgnoreCase))
            {
                throw new DuplicateClientException(ex);
            }
        }
    }
}
