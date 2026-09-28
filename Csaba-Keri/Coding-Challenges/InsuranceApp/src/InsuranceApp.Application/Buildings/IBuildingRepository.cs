using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Application.Buildings;

public interface IBuildingRepository
{
    Task<Building?> GetBuildingByIdAsync(Guid buildingId, CancellationToken cancellationToken);

    Task<PagedResult<Building>> GetBuildingsByClientIdAsync(Guid clientId, PageQuery query, CancellationToken cancellationToken);

    Task AddBuildingAsync(Building building, CancellationToken cancellationToken);

    Task UpdateBuildingAsync(Building building, CancellationToken cancellationToken);
}
