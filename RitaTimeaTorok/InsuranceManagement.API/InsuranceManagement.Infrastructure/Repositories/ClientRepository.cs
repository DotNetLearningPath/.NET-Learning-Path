using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class ClientRepository(InsuranceDbContext context)
    : GenericRepository<Client>(context), IClientRepository
    {
        public async Task<PagedResult<Client>> SearchAsync(
            string? name,
            string? identifier,
            int pageNumber,
            int pageSize)
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

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(client => client.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Client>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
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
