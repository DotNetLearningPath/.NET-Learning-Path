using Application.DTO.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

public interface IRiskFactorRepository
{
    Task AddRiskFactorAsync(
        RiskFactorConfiguration configuration,
        CancellationToken cancellationToken = default);

    Task<RiskFactorConfiguration?> GetByLevelAndReferenceAsync(
        RiskFactorLevel level,
        string reference,
        CancellationToken cancellationToken = default);

    Task<RiskFactorConfiguration?> GetByBuildingTypeAsync(
        BuildingType buildingType,
        CancellationToken cancellationToken = default);

    Task<PagedResult<RiskFactorConfiguration>> ListRiskFactorsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);
}
