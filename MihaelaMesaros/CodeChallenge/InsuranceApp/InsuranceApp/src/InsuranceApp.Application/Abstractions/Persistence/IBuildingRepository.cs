using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IBuildingRepository
{
    Task<Building?> GetBuildingByIdAsync(Guid buildingId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Building>> GetBuildingsByClientAsync(Guid clientId, CancellationToken cancellationToken);

    Task AddBuildingAsync(Building building, CancellationToken cancellationToken);

    Task SaveBuildingChangesAsync(CancellationToken cancellationToken);

    Task<Building?> GetBuildingForUpdateAsync(Guid buildingId, CancellationToken cancellationToken);

    Task<bool> BuildingTypeExistsAsync(Guid buildingTypeId, CancellationToken cancellationToken);
}