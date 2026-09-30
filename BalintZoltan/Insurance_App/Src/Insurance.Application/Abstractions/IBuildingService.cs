using Insurance.Application.DTO.Buildings;
using Insurance.Application.DTO.Common;

namespace Insurance.Application.Abstractions;

public interface IBuildingService
{
    Task<BuildingDto> CreateBuildingAsync(CreateBuildingRequest request, CancellationToken cancellationToken);
    Task<BuildingDto?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<BuildingDto>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task<BuildingDto> UpdateBuildingAsync(Guid id, UpdateBuildingRequest request, CancellationToken cancellationToken);
}
