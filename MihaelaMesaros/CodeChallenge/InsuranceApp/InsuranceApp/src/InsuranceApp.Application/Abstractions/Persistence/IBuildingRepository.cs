using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IBuildingRepository
{
    Task<Building?> GetBuildingByIdAsync(int buildingId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Building>> GetBuildingsByClientAsync(int clientId, CancellationToken cancellationToken);

    Task AddBuildingAsync(Building building, CancellationToken cancellationToken);

    Task SaveBuildingChangesAsync(CancellationToken cancellationToken);

    Task<Building?> GetBuildingForUpdateAsync(int buildingId, CancellationToken cancellationToken);
}