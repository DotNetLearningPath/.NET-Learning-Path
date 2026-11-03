using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class BuildingRepository(InsuranceDbContext context)
    : GenericRepository<Building>(context), IBuildingRepository
    {
        public Task<bool> ExistsByCityIdAsync(Guid cityId) =>
            _context.Buildings.AnyAsync(b => b.CityId == cityId);

        public Task<bool> ExistsByClientIdAsync(Guid clientId) =>
            _context.Buildings.AnyAsync(b => b.ClientId == clientId);
    }
}
