using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly InsuranceDbContext _context;

        public BuildingRepository(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Buildings.Include(building => building.City).
                ThenInclude(city => city.County).ThenInclude(county => county.Country).
                FirstOrDefaultAsync(building => building.Id == id,cancellationToken);
        }

        public async Task<IReadOnlyList<Building>> GetByClientIdAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken)
        {
            return await _context.Buildings.AsNoTracking()
                .Where(building => building.ClientId == clientId)
                .OrderBy(building => building.Street)
                .ThenBy(building => building.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Building building, CancellationToken cancellationToken)
        {
            await _context.Buildings.AddAsync(building, cancellationToken);
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
