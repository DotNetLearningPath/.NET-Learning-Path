using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Building;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IBuildingService
{
    Task<Result<BuildingDto>> GetBuildingByIdAsync(Guid buildingId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<BuildingDto>>> GetBuildingsByClientAsync(Guid clientId, CancellationToken cancellationToken);

    Task<Result<BuildingDto>> CreateBuildingForClientAsync(Guid clientId, CreateBuildingDto createBuildingDto, CancellationToken cancellationToken);

    Task<Result<BuildingDto>> UpdateBuildingAsync(Guid buildingId, UpdateBuildingDto updateBuildingDto, CancellationToken cancellationToken);
}