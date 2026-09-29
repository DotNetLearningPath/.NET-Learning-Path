using Application.DTO.Common;
using Domain.Entities;

namespace Application.Abstractions;

public interface IBuildingRepository
{
    Task<Building?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Building>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task AddBuildingAsync(Building building, CancellationToken cancellationToken = default);
    Task UpdateBuildingAsync(Building building, CancellationToken cancellationToken = default);
}
