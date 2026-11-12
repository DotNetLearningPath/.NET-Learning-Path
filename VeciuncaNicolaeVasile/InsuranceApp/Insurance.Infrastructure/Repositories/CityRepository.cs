using Insurance.Application.Interfaces;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories
{
    public class CityRepository : ICityRepository
    {
        private readonly InsuranceDbContext _context;

        public CityRepository(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(Guid cityId, CancellationToken cancellationToken)
        {
            return await _context.Cities.AnyAsync(city => city.Id == cityId, cancellationToken);
        }
    }
}