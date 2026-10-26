using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly InsuranceDbContext _context;

        public ClientRepository(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<List<Client>> SearchAsync(string? name, string? identificationNumber, int page, int pageSize, CancellationToken cancellationToken)
        {
            IQueryable<Client> query = _context.Clients.AsNoTracking();


            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(identificationNumber))
            {
                query = query.Where(c => c.IdentificationNumber == identificationNumber);
            }

            return await query.OrderBy(c => c.Name)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken)
        {
            return await _context.Clients
                .AnyAsync(c => c.IdentificationNumber == identificationNumber, cancellationToken);
        }

        public async Task AddAsync(Client client, CancellationToken cancellationToken)
        {
            await _context.Clients.AddAsync(client, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
