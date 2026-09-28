using InsuranceApp.Application.Buildings.Commands;
using InsuranceApp.Application.Buildings.Results;
using InsuranceApp.Application.Common.Pagination;

namespace InsuranceApp.Application.Buildings;

public interface IBuildingService
{
    Task<BuildingDetailsResult> GetBuildingDetailsByIdAsync(Guid buildingId, CancellationToken cancellationToken);

    Task<PagedResult<BuildingResult>> GetBuildingsByClientIdAsync(Guid clientId, PageQuery query, CancellationToken cancellationToken);

    Task<BuildingDetailsResult> CreateBuildingAsync(CreateBuildingCommand command, CancellationToken cancellationToken);

    Task<BuildingDetailsResult> UpdateBuildingAsync(UpdateBuildingCommand command, CancellationToken cancellationToken);
}
