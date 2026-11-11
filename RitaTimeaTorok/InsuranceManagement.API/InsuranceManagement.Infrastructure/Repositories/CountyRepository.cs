using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class CountyRepository(InsuranceDbContext context)
        : GenericRepository<County>(context), ICountyRepository
    {
        public Task<bool> ExistsByCountryIdAsync(Guid countryId) =>
            _context.Counties.AnyAsync(county => county.CountryId == countryId);
    }
}
