using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class CityRepository(InsuranceDbContext context)
        : GenericRepository<City>(context), ICityRepository
    {
        public Task<bool> ExistsByCountyIdAsync(Guid countyId) =>
            _context.Cities.AnyAsync(city => city.CountyId == countyId);
    }
}
