using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class BuildingRepository(
    InsuranceDbContext dbContext) : IBuildingRepository
{
    public async Task AddBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        await dbContext.Buildings.AddAsync(building, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Building?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Buildings
            .AsNoTracking()
            .FirstOrDefaultAsync(building => building.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Building>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Buildings
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
        dbContext.Buildings.Update(building);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
