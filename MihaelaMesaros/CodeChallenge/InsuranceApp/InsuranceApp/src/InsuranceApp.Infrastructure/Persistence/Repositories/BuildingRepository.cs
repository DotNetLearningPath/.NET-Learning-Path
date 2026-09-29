using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class BuildingRepository(InsuranceDbContext dbContext) : IBuildingRepository
{
    public Task<Building?> GetBuildingByIdAsync(int buildingId, CancellationToken cancellationToken)
    {
        return dbContext.Buildings.AsNoTracking().FirstOrDefaultAsync(x => x.BuildingId == buildingId, cancellationToken);
    }

    public async Task<IReadOnlyList<Building>> GetBuildingsByClientAsync(int clientId, CancellationToken cancellationToken)
    {
        return await dbContext.Buildings
            .AsNoTracking()
            .Where(x => x.ClientId == clientId)
            .OrderBy(x => x.BuildingId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        dbContext.Buildings.Add(building);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveBuildingChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Building?> GetBuildingForUpdateAsync(int buildingId, CancellationToken cancellationToken)
    {
        return dbContext.Buildings.FirstOrDefaultAsync(x => x.BuildingId == buildingId, cancellationToken);
    }
}