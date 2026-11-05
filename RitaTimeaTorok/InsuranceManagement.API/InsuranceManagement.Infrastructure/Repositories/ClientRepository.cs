using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class ClientRepository(InsuranceDbContext context)
    : GenericRepository<Client>(context), IClientRepository
    {
        public Task<List<Client>> SearchAsync(string? name, string? identifier)
        {
            var query = _context.Clients.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameFilter = name.Trim();
                query = query.Where(client => client.Name.Contains(nameFilter));
            }

            if (!string.IsNullOrWhiteSpace(identifier))
            {
                var identifierFilter = identifier.Trim();
                query = query.Where(client => client.IdentificationNumber == identifierFilter);
            }

            return query.ToListAsync();
        }

        public Task<bool> ExistsByIdentificationNumberAsync(
            string identificationNumber,
            Guid? excludeClientId = null)
        {
            return _context.Clients.AnyAsync(c =>
                c.IdentificationNumber == identificationNumber &&
                (!excludeClientId.HasValue || c.Id != excludeClientId.Value));
        }
    }
}
