using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.Application.Abstractions;

public interface IRiskFactorRepository
{
    Task AddRiskFactorAsync(
        RiskFactorConfiguration configuration,
        CancellationToken cancellationToken);

    Task<RiskFactorConfiguration?> GetRiskFactorByLevelAndReferenceAsync(
        RiskFactorLevel level,
        string reference,
        CancellationToken cancellationToken);

    Task<RiskFactorConfiguration?> GetRiskFactorByBuildingTypeAsync(
        BuildingType buildingType,
        CancellationToken cancellationToken);

    Task<RiskFactorConfiguration?> GetRiskFactorByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task UpdateRiskFactorAsync(
        RiskFactorConfiguration configuration,
        CancellationToken cancellationToken);

    Task<List<RiskFactorConfiguration>> GetApplicableRiskFactorsAsync(
        Guid? countryId,
        Guid? countyId,
        Guid? cityId,
        BuildingType? buildingType,
        CancellationToken cancellationToken);

    Task<PagedResult<RiskFactorConfiguration>> ListRiskFactorsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);
}
