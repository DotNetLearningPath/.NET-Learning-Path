using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class ClientRepository(InsuranceDbContext context)
    : GenericRepository<Client>(context), IClientRepository
    {
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
