using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Building;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IBuildingService
{
    Task<Result<BuildingDto>> GetBuildingByIdAsync(int buildingId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<BuildingDto>>> GetBuildingsByClientAsync(int clientId, CancellationToken cancellationToken);

    Task<Result<BuildingDto>> CreateBuildingForClientAsync(int clientId, CreateBuildingDto createBuildingDto, CancellationToken cancellationToken);

    Task<Result<BuildingDto>> UpdateBuildingAsync(int buildingId, UpdateBuildingDto updateBuildingDto, CancellationToken cancellationToken);
}