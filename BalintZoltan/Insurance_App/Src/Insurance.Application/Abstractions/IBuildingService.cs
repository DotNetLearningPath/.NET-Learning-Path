using Application.DTO.Buildings;
using Application.DTO.Common;

namespace Application.Abstractions;

public interface IBuildingService
{
    Task<BuildingDto> CreateBuildingAsync(CreateBuildingRequest request, CancellationToken cancellationToken = default);
    Task<BuildingDto?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<BuildingDto>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<BuildingDto> UpdateBuildingAsync(Guid id, UpdateBuildingRequest request, CancellationToken cancellationToken = default);
}
