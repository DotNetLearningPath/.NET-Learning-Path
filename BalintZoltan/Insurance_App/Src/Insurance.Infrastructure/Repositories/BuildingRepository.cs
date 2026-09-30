using Insurance.Domain.Entities;
using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class BuildingRepository : IBuildingRepository
{
    private readonly InsuranceDbContext _dbContext;

    public BuildingRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        await _dbContext.Buildings.AddAsync(building, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Building?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Buildings
            .AsNoTracking()
            .FirstOrDefaultAsync(building => building.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Building>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Buildings
            .AsNoTracking()
            .Where(building => building.ClientId == clientId);

        return await query
            .OrderBy(building => building.Street)
            .ThenBy(building => building.Number)
            .ThenBy(building => building.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task UpdateBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        _dbContext.Buildings.Update(building);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
