using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;

namespace Insurance.Application.Abstractions;

public interface IBuildingRepository
{
    Task<Building?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<Building>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task AddBuildingAsync(Building building, CancellationToken cancellationToken);
    Task UpdateBuildingAsync(Building building, CancellationToken cancellationToken);
}
